using Common.AppsettingsModels;
using Common.ProvenanceModels;
using Common.TieModels;
using Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.FusekiService;

namespace Services.ProvenanceService;

public class ProvenanceService : IProvenanceService
{
    private readonly IConfiguration _configuration;
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<ProvenanceService> _logger;

    public ProvenanceService(IConfiguration configuration,
                             IFusekiService fusekiService,
                             ILogger<ProvenanceService> logger)
    {
        _configuration = configuration;
        _fusekiService = fusekiService;
        _logger = logger;
    }

    public async Task<Provenance> CreateProvenanceFromTieMessage(TieData tieData)
    {
        var previousRevisions = await GetPreviousRevision(tieData);
        _logger.LogDebug("<ProvenanceService> - CreateProvenanceFromTieMessage: Retrieved {count} previous versions", previousRevisions.Count);
        
        var revisionName = GetRevisionName(tieData);
        var revisionDate = GetRevisionDate(tieData);

        RevisionStatus revisionStatus;
        int revisionNumber;

        var existingRevision = previousRevisions.Count > 0 ? previousRevisions.FirstOrDefault(r => r.RevisionName?.ToLower() == revisionName.ToLower()) : null;
        var latestRevision = previousRevisions.Count > 0 ? previousRevisions.Aggregate((r1, r2) => r1.RevisionNumber > r2.RevisionNumber ? r1 : r2) : null;

        if (existingRevision == null && (latestRevision == null || latestRevision.RevisionDate <= revisionDate))
        {
            revisionStatus = RevisionStatus.New;
            revisionNumber = latestRevision != null ? latestRevision.RevisionNumber + 1 : 1;
        }
        else if (existingRevision == null && latestRevision != null && latestRevision.RevisionDate > revisionDate)
        {
            revisionStatus = RevisionStatus.Unknown;
            revisionNumber = -1;
        }
        else if (existingRevision != null && latestRevision != null && latestRevision.RevisionDate <= revisionDate)
        {
            revisionStatus = RevisionStatus.Update;
            revisionNumber = latestRevision.RevisionNumber;
        }
        else if (existingRevision != null && latestRevision != null && latestRevision.RevisionDate > revisionDate)
        {
            revisionStatus = RevisionStatus.Old;
            revisionNumber = -1;
        }
        else
        {
            throw new InvalidOperationException("Unable to establish provenance information");
        }

        _logger.LogInformation("<ProvenanceService> - CreateProvenanceFromTieMessage: Generated revision status: '{status}'", revisionStatus);

        var provenance = new Provenance(GetFacilityIdentifier(tieData).ToLower(), GetDataSource());

        provenance.DocumentProjectId = GetDocumentProjectId(tieData);
            provenance.PlantId = GetPlantId();
            provenance.DataCollectionName = GetDataCollectionName(tieData);
            provenance.RevisionName = revisionName;
            provenance.RevisionNumber = revisionNumber;
            provenance.PreviousRevision = latestRevision?.FullName ?? null;
            provenance.RevisionDate = revisionDate;
            provenance.RevisionStatus = revisionStatus;
            provenance.DataSourceType = GetDataSourceType();
            provenance.Contractor = GetContractor(tieData);

        return provenance;
    }

    private string GetFacilityIdentifier(TieData tieData)
    {
        return tieData.InterfaceData.Site ??
                    (tieData.ObjectData.InstallationCode ??
                    throw new ArgumentException("Tie data doesn't contain facility id"));
    }

    private string GetDocumentProjectId(TieData tieData)
    {
        var objectName = tieData.InterfaceData.ObjectName ?? throw new ArgumentException("Tie data doesn't contain document project id");
        var documentProjectId = tieData.InterfaceData.ObjectName.Substring(0, tieData.InterfaceData.ObjectName.IndexOf("-"));

        return documentProjectId ;
    }

    private string GetPlantId()
    {
        return "NA";
    }

    private string GetDataCollectionName(TieData tieData)
    {
        return tieData.FileData.Name;
    }
    private string GetRevisionName(TieData tieData)
    {
        return tieData.ObjectData.RevisionNumber ?? throw new ArgumentException("Tie data doesn't contain revision number");
    }

    private async Task<List<RevisionInfo>> GetPreviousRevision(TieData tieData)
    {
        var rdfServerName = ServerKeys.Dugtrio;

        _logger.LogDebug("ProvenanceService> - GetPreviousRevision: Retrieving data from {server}", rdfServerName);

        string documentProjectId = GetDocumentProjectId(tieData);
        string query = GetRevisionInfoQuery(documentProjectId);

        var response = await _fusekiService.QueryFusekiResponseAsApp(rdfServerName, query);

        List<RevisionInfo> previousRevisions = new List<RevisionInfo>();

        if (response.Results.Bindings.Count == 0)
        {
            return previousRevisions;
        }

        var revisions = response.Results.Bindings.SelectMany(x => x).ToList();
        var revisionInfo = new RevisionInfo();

        foreach (var revision in revisions)
        {
            switch (revision.Key)
            {
                case "revision":
                    {
                        if (revisionInfo.FullName != null)
                        {
                            previousRevisions.Add(revisionInfo);
                        }

                        revisionInfo = new RevisionInfo();
                        revisionInfo.FullName = revision.Value?.Value != null ? new Uri(revision.Value.Value) : null;
                        break;
                    }
                case "revisionDate":
                    {
                        revisionInfo.RevisionDate = revision.Value?.Value != null ? DateTime.Parse(revision.Value.Value) : null;
                        break;
                    }
                case "revisionName":
                    {
                        revisionInfo.RevisionName = revision.Value?.Value ?? null;
                        break;
                    }
                case "revisionNumber":
                    {
                        revisionInfo.RevisionNumber = revision.Value?.Value != null ? Int32.Parse(revision.Value.Value) : 0;
                        break;
                    }
                default:
                    {
                        _logger.LogWarning("<ProvenanceService> - GetPreviousRevision: Unknown revision property");
                        break;
                    }
            }
        }

        if (revisionInfo.FullName != null)
        {
            previousRevisions.Add(revisionInfo);
        }

        return previousRevisions;
    }

    private DateTime GetRevisionDate(TieData tieData)
    {
        DateTime result;
        bool success = DateTime.TryParse(tieData.ObjectData.RevisionDate, out result);

        if (!success)
        {
            throw new ArgumentException("Failed to convert revision date to DateTime");
        }
        return result;
    }

    private string GetDataSource()
    {
        return DataSource.Mel();
    }

    private string GetDataSourceType()
    {
        return DataSourceType.File();
    }

    private string GetContractor(TieData tieData)
    {
        return tieData.ObjectData.ContractorCode ?? "NA";
    }

    private string GetRevisionInfoQuery(string documentProjectId)
    {
        return @$"prefix sor: <https://rdf.equinor.com/ontology/sor#>
                          prefix facility: <https://rdf.equinor.com/ontology/facility#>
                          prefix prov: <http://www.w3.org/ns/prov#>

                    SELECT ?revision ?revisionDate ?revisionNumber ?revisionName
                    WHERE 
                    {{
                        ?revision facility:hasDocumentProjectId ?DocumentProjectId ;        
                            prov:generatedAtTime ?revisionDate .

                        OPTIONAL {{?revision sor:hasRevisionNumber ?revisionNumber .}}
                        OPTIONAL {{?revision sor:hasRevisionName ?revisionName .}}

                        FILTER (?DocumentProjectId = facility:{documentProjectId}) 
                    }}";
    }

}
