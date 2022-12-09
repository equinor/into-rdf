using Services.FusekiServices;
using VDS.RDF;

namespace Repositories.OntologyRepository;

public class OntologyRepository : IOntologyRepository
{
    private readonly IFusekiQueryService _fusekiQueryService;
    public OntologyRepository(IFusekiQueryService fusekiQueryService)
    {
        _fusekiQueryService = fusekiQueryService;
    }
    public async Task<Graph> Get(string server, string trainType)
    {
        string query = GetConstructQuery(trainType);
        return await _fusekiQueryService.Construct(server, query);
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
