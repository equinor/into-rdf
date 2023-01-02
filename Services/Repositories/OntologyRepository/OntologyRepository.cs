using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using VDS.RDF;
using VDS.RDF.Query;

namespace Repositories.OntologyRepository;

public class OntologyRepository : IOntologyRepository
{
    private readonly IFusekiQueryService _fusekiQueryService;
    private readonly ILogger<OntologyRepository> _log;
    public OntologyRepository(IFusekiQueryService fusekiQueryService, ILogger<OntologyRepository> log)
    {
        _fusekiQueryService = fusekiQueryService;
        _log = log;
    }

    public async Task<Graph> Get(string server, string trainType)
    {
        string query = GetConstructQuery(trainType);

        var ontologyGraph = await _fusekiQueryService.Construct(server, query);
        
        _log.LogInformation(!ontologyGraph.IsEmpty ? $"Successfully retrieved ontologies" : $"Failed to retrieve ontologies");
        
        return ontologyGraph;
    }

    private string GetConstructQuery(string source)
    {
        var vocabulary = new Uri($"https://rdf.equinor.com/graph/source/{source}");
        var ontology = new Uri($"https://rdf.equinor.com/graph/ontology/{source}");

        var queryString = new SparqlParameterizedString();
        queryString.CommandText =
        @$"CONSTRUCT 
            {{
                ?a ?b ?c .
            }}
            FROM NAMED @vocabulary
            FROM NAMED @ontology
            WHERE 
            {{
                GRAPH ?g 
                {{
                    ?a ?b ?c . 
                }}
            }}";

        queryString.SetUri("vocabulary", vocabulary);
        queryString.SetUri("ontology", ontology);

        return queryString.ToString();
    }
}
