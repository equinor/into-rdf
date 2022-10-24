using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.TieModels;

namespace Services.ProvenanceServices;
public interface IProvenanceService
{
    Provenance CreateProvenanceFromTieMessage(string datasource, List<RevisionInfo> previousRevisions, TieData tieData);
    Provenance CreateProvenanceFromSpreadsheetInfo(List<RevisionInfo> previousRevisions, SpreadsheetInfo info);
    Provenance CreateProvenanceFromCommonLib(string library, List<RevisionInfo> previousRevisions, RevisionRequirement requirement);
    Task<List<RevisionInfo>> GetPreviousRevisions(string server, string revisionTrain);
}