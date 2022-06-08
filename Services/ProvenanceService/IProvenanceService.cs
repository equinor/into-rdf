using Common.TieModels;
using Common.ProvenanceModels;

namespace Services.ProvenanceService;
public interface IProvenanceService
{
    public Task<Provenance> CreateProvenanceFromTieMessage(TieData tieData);
}