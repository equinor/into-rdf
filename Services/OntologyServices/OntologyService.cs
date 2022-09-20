using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Services.OntologyServices.OntologyService;
public class OntologyService : IOntologyService
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<OntologyService> _logger;

    public OntologyService(IFusekiService fusekiService, ILogger<OntologyService> logger)
    {
        _fusekiService = fusekiService;
        _logger = logger;
    }
    public async Task<Graph> GetSourceOntologies(string source)
    {
        string query = GetConstructQuery(source);
        var result = await _fusekiService.QueryAsApp(ServerKeys.SplinterConfig, query);

        _logger.LogDebug(result != null ? $"Successfully retrieved {source} ontologies" : $"Failed to retrieve ontologies for {source}");

        Graph graph = new Graph();
        graph.LoadFromString(result, new TurtleParser());
        return graph;
    }

    private string GetConstructQuery(string source)
    {
        return @$"CONSTRUCT 
            {{
                ?a ?b ?c .
            }}
            FROM NAMED <https://rdf.equinor.com/graph/source/{source}>
            FROM NAMED <https://rdf.equinor.com/graph/ontology/{source}>
            WHERE 
            {{
                GRAPH ?g 
                {{
                    ?a ?b ?c . 
                }}
            }}";
    }
}

