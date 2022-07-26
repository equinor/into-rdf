using Common.TieModels;
using Common.ProvenanceModels;

namespace Services.ProvenanceServices;
public interface IProvenanceService
{
    public Task<Provenance> CreateProvenanceFromTieMessage(TieData tieData);
}