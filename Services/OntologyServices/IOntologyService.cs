using Common.ProvenanceModels;
using VDS.RDF;

namespace Services.OntologyServices.OntologyService
{
    public interface IOntologyService
    {
        public Task<Graph> GetSourceOntologies(string source);
    }
}