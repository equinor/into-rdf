using Common.FacilityModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.TieModels;
using Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;

namespace Services.ProvenanceServices;

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

    public async Task<Provenance> CreateProvenanceFromTieMessage(string datasource, TieData tieData)
    {
        var revisionRequirement = tieData.GetRevisionRequirements();

        _logger.LogDebug("<ProvenanceService> - CreateProvenanceFromTieMessage: Creating provenance for project {pdi} with revision name {rn} dated {rd}",
                                revisionRequirement.DocumentProjectId, revisionRequirement.RevisionName, revisionRequirement.RevisionDate);

        var provenance = await CreateProvenance(tieData.GetDataCollectionName(), datasource, revisionRequirement);
        provenance.Contractor = tieData.GetContractor();

        return provenance;
    }

    public async Task<Provenance> CreateProvenanceFromSpreadsheetInfo(SpreadsheetInfo info)
    {
        if (info.FileName == null || info.DataSource == null)
        {
            throw new InvalidOperationException("Not sufficient info about spreadsheet to create provenance");
        }

        var revisionRequirement = info.GetRevisionRequirements();

        _logger.LogDebug("<ProvenanceService> - CreateProvenanceFromSpreadsheetInfo: Creating provenance for project {pdi} with revision name {rn} dated {rd}",
            revisionRequirement.DocumentProjectId, revisionRequirement.RevisionName, revisionRequirement.RevisionDate);

        var provenance = await CreateProvenance(info.FileName, info.DataSource.ToLower(), revisionRequirement);

        return provenance;
    }

    private async Task<Provenance> CreateProvenance(string filename, string datasource, RevisionRequirement revisionRequirement)
    {
        var facility = await GetFacilityId(revisionRequirement.DocumentProjectId);
        var facilityId = facility.Label ?? throw new InvalidOperationException("Failed to retrieve facilityId when generating provenance");

        _logger.LogDebug("<ProvenanceService> - CreateProvenance: Prepare to retrieve previous revisions for facilityId {facilityId}", facility.Label);

        var previousRevisions = await GetPreviousRevision(revisionRequirement.DocumentProjectId, datasource);

        _logger.LogDebug("<ProvenanceService> - CreateProvenance: Retrieved {count} previous versions", previousRevisions.Count);

        var existingRevision = previousRevisions.Count > 0 ? previousRevisions.FirstOrDefault(r => r.RevisionName?.ToLower() == revisionRequirement.RevisionName.ToLower()) : null;
        var latestRevision = previousRevisions.Count > 0 ? previousRevisions.Aggregate((r1, r2) => r1.RevisionNumber > r2.RevisionNumber ? r1 : r2) : null;

        GetRevisionStatus(existingRevision, latestRevision, revisionRequirement.RevisionDate, out var revisionStatus, out var revisionNumber);

        _logger.LogDebug("<ProvenanceService> - CreateProvenance: Latest revision number is {number}", latestRevision?.RevisionNumber);
        _logger.LogInformation("<ProvenanceService> - CreateProvenance: Generated revision status: '{status}'", revisionStatus);

        var provenance = new Provenance(facilityId, datasource);
        provenance.DocumentProjectId = revisionRequirement.DocumentProjectId;
        provenance.DataCollectionName = filename;
        provenance.RevisionName = revisionRequirement.RevisionName;
        provenance.RevisionNumber = revisionNumber;
        provenance.PreviousRevision = latestRevision?.FullName ?? null;
        provenance.RevisionDate = revisionRequirement.RevisionDate;
        provenance.RevisionStatus = revisionStatus;
        provenance.DataSourceType = DataSourceType.File();

        return provenance;
    }

    private async Task<FacilityId> GetFacilityId(string documentProjectId)
    {
        var rdfServerName = ServerKeys.SplinterConfig;

        string query = GetFacilityIdQuery(documentProjectId);
        var response = await _fusekiService.QueryFusekiResponseAsApp<FacilityId>(rdfServerName, query);

        return response.Count == 1 ? response[0] : throw new InvalidOperationException("Wrong number of FacilityIds returned");
    }

    private async Task<List<RevisionInfo>> GetPreviousRevision(string documentProjectId, string datasource)
    {
        var rdfServerName = ServerKeys.Dugtrio;

        _logger.LogDebug("<ProvenanceService> - GetPreviousRevision: Retrieving data from {server}", rdfServerName);
        _logger.LogDebug("<ProvenanceService> - GetPreviousRevision: Current document project id {id}", documentProjectId);

        string query = datasource == DataSource.Mel ? GetRevisionInfoQuery(documentProjectId) : GetRevisionInfoFromNamedGraphQuery(documentProjectId);

        return await _fusekiService.QueryFusekiResponseAsApp<RevisionInfo>(rdfServerName, query);
    }

    private void GetRevisionStatus(RevisionInfo? existingRevision, RevisionInfo? latestRevision, DateTime revisionDate, out RevisionStatus revisionStatus, out int revisionNumber)
    {
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
    }

    private string GetRevisionInfoQuery(string documentProjectId)
    {
        return @$"
            prefix sor: <https://rdf.equinor.com/ontology/sor#>
            prefix identification: <https://rdf.equinor.com/ontology/facility-identification/v1#>
            prefix identifier: <https://rdf.equinor.com/data/facility-identification/>
            prefix prov: <http://www.w3.org/ns/prov#>

                    SELECT ?{nameof(RevisionInfo.FullName)} ?{nameof(RevisionInfo.RevisionDate)} ?{nameof(RevisionInfo.RevisionNumber)} ?{nameof(RevisionInfo.RevisionName)}
                    WHERE 
                    {{
                        ?{nameof(RevisionInfo.FullName)} identification:hasDocumentProjectId ?DocumentProjectId ;        
                            prov:generatedAtTime ?{nameof(RevisionInfo.RevisionDate)} .

                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionNumber ?{nameof(RevisionInfo.RevisionNumber)} .}}
                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionName ?{nameof(RevisionInfo.RevisionName)} .}}

                        FILTER (?DocumentProjectId = identifier:{documentProjectId}) 
                    }}";
    }

    private string GetRevisionInfoFromNamedGraphQuery(string documentProjectId)
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
                            ?{nameof(RevisionInfo.FullName)} identification:hasDocumentProjectId ?DocumentProjectId ;        
                            prov:generatedAtTime ?{nameof(RevisionInfo.RevisionDate)} .

                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionNumber ?{nameof(RevisionInfo.RevisionNumber)} .}}
                        OPTIONAL {{?{nameof(RevisionInfo.FullName)} sor:hasRevisionName ?{nameof(RevisionInfo.RevisionName)} .}}

                        FILTER (?DocumentProjectId = identifier:{documentProjectId})
                        }} 
                    }}";
    }

    private string GetFacilityIdQuery(string documentProjectId)
    {
        return @$"
            prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            prefix identification: <https://rdf.equinor.com/ontology/facility-identification/v1#>
            prefix identifier: <https://rdf.equinor.com/data/facility-identification/>

                    SELECT ?{nameof(FacilityId.Label)}
                    FROM NAMED <https://rdf.equinor.com/graph/data/identifiers>
                    WHERE 
                    {{
                        GRAPH ?g
                        {{
                            ?facility identification:hasDocumentProjectId identifier:{documentProjectId} ;        
                            identification:hasFacilityId ?FacilityId .

                            ?FacilityId rdfs:label ?{nameof(FacilityId.Label)} .
                        }}
                    }}";
    }
}
