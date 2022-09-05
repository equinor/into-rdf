using System.Net.Http.Headers;
using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.RdfServices;

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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/turtle", "application/sparql-results+json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(int.MaxValue)]
        [HttpPost("{server}/sparql")]
        public async Task<ContentResult> Query(string server, [FromBody] SparqlQuery sparql)
        {
            var response = await _rdfService.QueryFusekiAsUser(server, sparql.Query);
            var content = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType;
            return Content(content, contentType ?? "text/plain", System.Text.Encoding.UTF8);
        }
    }
    public record SparqlQuery(string Query);
}
