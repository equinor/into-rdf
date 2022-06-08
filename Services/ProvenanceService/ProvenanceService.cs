using Common.AppsettingsModels;
using Common.ProvenanceModels;
using Common.TieModels;
using Microsoft.Extensions.Configuration;
using Services.FusekiService;

namespace Services.ProvenanceService;

public class ProvenanceService : IProvenanceService
{
    private readonly IConfiguration _configuration;
    private readonly IFusekiService _fusekiService;

    public ProvenanceService(IConfiguration configuration, IFusekiService fusekiService)
    {
        _configuration = configuration;
        _fusekiService = fusekiService;
    }

    public async Task<Provenance> CreateProvenanceFromTieMessage(TieData tieData)
    {
        var previousRevision = await GetPreviousRevision(tieData);
        var revisionName = GetRevisionName(tieData);
        var revisionDate = GetRevisionDate(tieData);
     
        string revisionStatus;
        int revisionNumber;

        if (previousRevision.RevisionName == revisionName || previousRevision.RevisionDate == revisionDate)
        {
            revisionStatus = "UPDATE";
            revisionNumber = previousRevision.RevisionNumber;
        }
        else if (previousRevision.RevisionNumber == 0 || previousRevision.RevisionNumber > 0 && previousRevision.RevisionDate < revisionDate)
        {
            revisionStatus = "NEW";
            revisionNumber = previousRevision.RevisionNumber + 1;
        }
        else
        {
            throw new InvalidOperationException("Unable to establish provenance information");
        }

        return new Provenance
        {
            FacilityId = GetFacilityIdentifier(tieData).ToLower(),
            DocumentProjectId = GetDocumentProjectId(tieData).ToLower(),
            PlantId = GetPlantId().ToLower(),
            DataCollectionName = GetDataCollectionName(tieData),
            RevisionName = revisionName.ToLower(),
            RevisionNumber = revisionNumber,
            PreviousRevision = previousRevision.FullName,
            RevisionDate = revisionDate,
            RevisionStatus = revisionStatus,
            DataSource = GetDataSource(),
            DataSourceType = GetDataSourceType(),
            DataSourceTable = GetDataSourceTable().ToLower(),
            Contractor = GetContractor(tieData),
        };
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
        var docProd = tieData.InterfaceData.ObjectName.Substring(0, tieData.InterfaceData.ObjectName.IndexOf("-"));

        return tieData.InterfaceData.ObjectName.Substring(0, tieData.InterfaceData.ObjectName.IndexOf("-"));
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

    private async Task<RevisionInfo> GetPreviousRevision(TieData tieData)
    {
        var rdfServerConf = _configuration.GetSection("Servers").Get<List<RdfServer>>();
        var rdfServerName = rdfServerConf[0].Name;

        string documentProjectId = GetDocumentProjectId(tieData);
        string query = GetRevisionInfoQuery(documentProjectId);

        var response = await _fusekiService.QueryFusekiServer(rdfServerName, query);

        var revisionInfo = new RevisionInfo();

        if (response.Results.Bindings.Count == 0)
        {
            revisionInfo.RevisionNumber = 0;
        }

        else
        {
            var triples = response.Results.Bindings.Single();

            Triplet? triple;
            var tripleExist = triples.TryGetValue("latestRevisionNumber", out triple);

            if (triple != null)
            {
                var revisionNumber = 0;
                var revisionExist = Int32.TryParse(triple.Value, out revisionNumber);
                revisionInfo.RevisionNumber = revisionNumber;
            }

            tripleExist = triples.TryGetValue("revision", out triple);
            revisionInfo.FullName = triple != null && triple.Value != null ? new Uri(triple.Value) : null;

            tripleExist = triples.TryGetValue("revisionDate", out triple);
            revisionInfo.RevisionDate = triple != null && triple.Value != null ? DateTime.Parse(triple.Value) : null;

            tripleExist = triples.TryGetValue("revisionName", out triple);
            revisionInfo.RevisionName = triple != null ? triple.Value : String.Empty; 
        }
        return revisionInfo;
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

    private string GetDataSourceTable()
    {
        return "NA";
    }

    private string GetContractor(TieData tieData)
    {
        return tieData.ObjectData.ContractorCode ?? "NA";
    }

    private string GetRevisionInfoQuery(string documentProjectId)
    {
        return @$"prefix sor: <http://rdf.equinor.com/ontology/sor#>
                          prefix facility: <http://rdf.equinor.com/ontology/facility#>
                          prefix prov: <http://www.w3.org/ns/prov#>

                    SELECT ?revision ?latestRevisionNumber ?revisionDate ?revisionName
                    WHERE 
                    {{
                        {{
                            SELECT ?DocumentProjectId (MAX(?revisionNumber) AS ?latestRevisionNumber)
                            WHERE
                            {{
                                ?dataset facility:hasDocumentProjectId ?DocumentProjectId  ;
                                     sor:hasRevisionNumber ?revisionNumber .

                                

                                 FILTER (?DocumentProjectId = facility:{documentProjectId}) 
                            }}
                            GROUP BY ?DocumentProjectId
                        }}

                        ?revision facility:hasDocumentProjectId ?DocumentProjectId  ;
                            sor:hasRevisionNumber ?latestRevisionNumber ;
                            prov:generatedAtTime ?revisionDate .

                        OPTIONAL {{?revision sor:hasRevisionName ?revisionName .}}
                    }}";
    }

}
