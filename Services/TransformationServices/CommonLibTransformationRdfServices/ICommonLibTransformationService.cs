using Common.GraphModels;
using Common.ProvenanceModels;
using VDS.RDF;

namespace Services.CommonLibToRdfServices;

public interface ICommonLibTransformationService
{
    ResultGraph Transform(Provenance provenance, Graph ontology, List<Dictionary<string, object>> records);
}