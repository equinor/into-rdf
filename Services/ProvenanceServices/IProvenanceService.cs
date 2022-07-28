using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.TieModels;

namespace Services.ProvenanceServices;
public interface IProvenanceService
{
    Task<Provenance> CreateProvenanceFromTieMessage(string datasource, TieData tieData);
    Task<Provenance> CreateProvenanceFromSpreadsheetInfo(SpreadsheetInfo info);
}