using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.TieModels;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;

namespace Services.ProvenanceServices;

public class ProvenanceService : IProvenanceService
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<ProvenanceService> _logger;

    public ProvenanceService(IFusekiService fusekiService,
                             ILogger<ProvenanceService> logger)
    {
        _fusekiService = fusekiService;
        _logger = logger;
    }

    public Provenance CreateProvenanceFromTieMessage(string datasource, List<RevisionInfo> previousRevisions, TieData tieData)
    {
        var revisionRequirement = tieData.GetRevisionRequirements();

        _logger.LogDebug("<ProvenanceService> - CreateProvenanceFromTieMessage: Creating provenance document {doc} for facility {fid} with revision name {rn} dated {rd}",
                                revisionRequirement.DocumentName, revisionRequirement.FacilityId , revisionRequirement.RevisionName, revisionRequirement.RevisionDate);

        var provenance = CreateProvenance(tieData.GetDataCollectionName(), datasource, previousRevisions, revisionRequirement);
        provenance.DocumentProjectId = tieData.GetDocumentProjectId();
        provenance.Contractor = tieData.GetContractor();
        provenance.ContractNumber = tieData.GetContractNumber();
        provenance.ProjectCode = tieData.GetProjectNumber();
        provenance.DocumentTitle = tieData.GetDocumentTitle();
        provenance.DataSourceType = DataSourceType.File();

        return provenance;
    }

    public Provenance CreateProvenanceFromSpreadsheetInfo(List<RevisionInfo> previousRevisions, SpreadsheetInfo info)
    {
        if (info.FileName == null || info.DataSource == null)
        {
            throw new InvalidOperationException("Not sufficient info about spreadsheet to create provenance");
        }

        var revisionRequirement = info.GetRevisionRequirements();

        _logger.LogDebug("<ProvenanceService> - CreateProvenanceFromTieMessage: Creating provenance document {doc} for facility {fid} with revision name {rn} dated {rd}",
                                revisionRequirement.DocumentName, revisionRequirement.FacilityId , revisionRequirement.RevisionName, revisionRequirement.RevisionDate);

        var provenance = CreateProvenance(info.FileName, info.DataSource.ToLower(), previousRevisions, revisionRequirement);
        provenance.DocumentProjectId = info.DocumentProjectId;
        provenance.DataSourceType = DataSourceType.File();

        return provenance;
    }

    public Provenance CreateProvenanceFromCommonLib(string library, List<RevisionInfo> previousRevisions, RevisionRequirement requirement)
    {
        var provenance = CreateProvenance(string.Empty, DataSource.CommonLib, previousRevisions, requirement);

        provenance.DataSourceTable = library;
        provenance.RevisionName = provenance.RevisionNumber.ToString();
        provenance.DataSourceType = DataSourceType.Database();

        return provenance;
    }

    private Provenance CreateProvenance(string filename, string datasource, List<RevisionInfo> previousRevisions, RevisionRequirement revisionRequirement)
    {
        var existingRevision = previousRevisions.Count > 0 ? previousRevisions.FirstOrDefault(r => r.RevisionName?.ToLower() == revisionRequirement.RevisionName.ToLower()) : null;
        var latestRevision = previousRevisions.Count > 0 ? previousRevisions.MaxBy(revision => revision.RevisionNumber) : null;

        GetRevisionStatus(existingRevision, latestRevision, revisionRequirement.RevisionDate, out var revisionStatus, out var revisionNumber);

        _logger.LogDebug("<ProvenanceService> - CreateProvenance: Latest revision number is {number}", latestRevision?.RevisionNumber);
        _logger.LogInformation("<ProvenanceService> - CreateProvenance: Generated revision status: '{status}'", revisionStatus);

        var provenance = new Provenance(revisionRequirement.FacilityId, datasource);
        provenance.DocumentName = revisionRequirement.DocumentName;
        provenance.DataCollectionName = filename;
        provenance.RevisionName = revisionRequirement.RevisionName;
        provenance.RevisionNumber = revisionNumber;
        provenance.PreviousRevision = latestRevision?.FullName ?? null;
        provenance.RevisionDate = revisionRequirement.RevisionDate;
        provenance.RevisionStatus = revisionStatus;
        return provenance;
    }

    public async Task<List<RevisionInfo>> GetPreviousRevisions(string server, string documentName)
    {
        _logger.LogDebug("<ProvenanceService> - GetPreviousRevisions: Get revisions for document {id} on server {server}", documentName, server);
        string query = GetRevisionInfoFromNamedGraphQuery(documentName);

        var previousRevisions = await _fusekiService.QueryFusekiResponseAsApp<RevisionInfo>(server, query);
        _logger.LogDebug("<ProvenanceService> - CreateProvenance: Retrieved {count} previous versions", previousRevisions.Count);
        
        return previousRevisions;
    }

    private void GetRevisionStatus(RevisionInfo? existingRevision, RevisionInfo? latestRevision, DateTime revisionDate, out RevisionStatus revisionStatus, out int revisionNumber)
    {
        if (existingRevision == null && (latestRevision == null || latestRevision.RevisionDate < revisionDate))
        {
            revisionStatus = RevisionStatus.New;
            revisionNumber = latestRevision != null ? latestRevision.RevisionNumber + 1 : 1;
        }
        else if (existingRevision == null && latestRevision != null && latestRevision.RevisionDate >= revisionDate)
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
    }

    private string GetRevisionInfoFromNamedGraphQuery(string documentName)
    {
        return @$"
            prefix sor: <https://rdf.equinor.com/ontology/sor#>
            prefix identification: <https://rdf.equinor.com/ontology/facility-identification/v1#>
            prefix identifier: <https://rdf.equinor.com/data/facility-identification/>
            prefix prov: <http://www.w3.org/ns/prov#>

                    SELECT ?g ?{nameof(RevisionInfo.FullName)} ?{nameof(RevisionInfo.RevisionDate)} ?{nameof(RevisionInfo.RevisionNumber)} ?{nameof(RevisionInfo.RevisionName)}
                    WHERE 
                    {{
                        Graph ?g 
                        {{
                            ?{nameof(RevisionInfo.FullName)} sor:hasDocumentName ?DocumentName ;        
                            prov:generatedAtTime ?{nameof(RevisionInfo.RevisionDate)} .

                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionNumber ?{nameof(RevisionInfo.RevisionNumber)} .}}
                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionName ?{nameof(RevisionInfo.RevisionName)} .}}

                        FILTER (STR(?DocumentName) = '{documentName}') 
                        }} 
                    }}";
    }
}