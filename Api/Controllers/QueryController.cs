using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.RdfService;

namespace Api.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IRdfService _rdfService;

        public QueryController(IRdfService rdfService)
        {
            _rdfService = rdfService;
        }

        /// <summary>
        /// Query fuseki
        /// </summary>
        /// <param name="server">specifies which fuseki to query from</param>
        /// <param name="sparql">SPARQL query</param>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(int.MaxValue)]
        [HttpPost("{server}/sparql")]
        public async Task<IActionResult> Query(string server, [FromBody] SparqlQuery sparql)
        {
            return Ok(await _rdfService.QueryFusekiAsUser(server, sparql.Query));
        }
    }

    public record SparqlQuery(string Query);
}
