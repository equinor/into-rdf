using VDS.RDF;

namespace Repositories.OntologyRepository
{
    public interface IOntologyRepository
    {
        Task<Graph> Get(string server, string trainType);
    }
}