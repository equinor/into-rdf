using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.TieModels;
using VDS.RDF;

namespace Services.ProvenanceServices;
public interface IProvenanceService
{
    Provenance CreateProvenanceFromCommonLib(string library, List<RevisionInfo> previousRevisions, RevisionRequirement requirement);
    Task<List<RevisionInfo>> GetPreviousRevisions(string server, string revisionTrain);
}