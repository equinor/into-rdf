using VDS.RDF;

namespace Services.FusekiServices;

public interface IFusekiQueryService
{
    Task<bool> Ask(string server, string sparql);
    Task<List<T>> Select<T>(string server, string sparql) where T : new();
    Task<Graph> Construct(string server, string sparql);
}