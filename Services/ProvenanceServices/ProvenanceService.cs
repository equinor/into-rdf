using Common.ProvenanceModels;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;

namespace Services.ProvenanceServices;
//TODO - Deprecated, to be removed when FunctionalSystems are moved over to a new Record endpoint.
public class ProvenanceService : IProvenanceService
{
    private readonly IFusekiQueryService _fusekiQueryService;
    private readonly ILogger<ProvenanceService> _logger;

    public ProvenanceService(IFusekiQueryService fusekiQueryService,
                             ILogger<ProvenanceService> logger)
    {
        _fusekiQueryService = fusekiQueryService;
        _logger = logger;
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

        var previousRevisions = await _fusekiQueryService.Select<RevisionInfo>(server, query);
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