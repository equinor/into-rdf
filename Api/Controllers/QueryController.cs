using System.Net.Http.Headers;
using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Writers;
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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/json")]
        [Produces("text/turtle", "application/trig", "application/sparql-results+json")]
        [Consumes("application/sparql-query", "application/json", "text/plain", "application/x-www-form-urlencoded", "multipart/form-data", "text/plain")]

        [RequestSizeLimit(int.MaxValue)]
        [HttpPost("{server}/sparql")]
        public async Task<ContentResult> Query(string server, [FromBody] SparqlQuery sparql)
        {
            var response = await _rdfService.QueryFusekiAsUser(server, sparql.Query, sparql.Accept);
            var content = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType;
            return Content(content, contentType ?? "text/plain", System.Text.Encoding.UTF8);
        }
    }
    public struct SparqlQuery
    {
        public SparqlQuery(string Query, IEnumerable<string?>? Accept = null)
        {
            this.Query = Query;
            this.Accept = Accept;
        }
        public string Query { get; }
        internal IEnumerable<string?>? Accept { get; }


        public static readonly string[] SupportedMediaTypes = { "application/sparql-query", "text/plain", "application/json", "application/x-www-form-urlencoded", "multipart/form-data" };

        /// <summary>
        /// Minimal-api special signature for converting types in http-handler singnatures
        /// e.g. [FromBody], [FromQuery]
        /// </summary>
        /// <param name="context">aspnet request context object</param>
        public static async ValueTask<SparqlQuery?> BindAsync(HttpContext context)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string? contentTypeHeader = context.Request.ContentType;
            if (MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var ContentType))
            {
                contentTypeHeader = ContentType.MediaType ?? contentTypeHeader;
                if (ContentType.CharSet != null) try
                    {
                        enc = System.Text.Encoding.GetEncoding(ContentType.CharSet);
                    }
                    catch { }
            }
            var accept = new Dictionary<string, MediaTypeHeaderValue>();
            bool star = false;
            foreach (var av in context.Request.Headers.Accept)
            {
                if (MediaTypeHeaderValue.TryParse(av, out var mv))
                {
                    if (mv.MediaType == "*/*")
                    {
                        star = true;
                        continue;
                    }
                    accept.Add(mv.MediaType ?? mv.GetHashCode().ToString(), mv);
                }
            }
            if (star && context.GetEndpoint() is Endpoint ep)
            {
                foreach (var ctype in ep.Metadata
                                    .OfType<ProducesAttribute>()
                                    .Where(p => p.StatusCode == 200)
                                    .Distinct()
                                    .SelectMany(p => p.ContentTypes))
                {
                    if (accept.ContainsKey(ctype)) continue;
                    accept.Add(ctype, new MediaTypeHeaderValue(ctype));
                }
            }
            if (accept.ContainsKey("text/turtle") && accept.ContainsKey("application/trig"))
            {
                if (!accept["text/turtle"].Parameters.Any(x => x.Name == "q"))
                    accept["text/turtle"].Parameters.Add(new NameValueHeaderValue("q", "0.8"));
                if (!accept["application/trig"].Parameters.Any(x => x.Name == "q"))
                    accept["application/trig"].Parameters.Add(new NameValueHeaderValue("q", "0.9"));
            }
            var ctypes = accept.Values.Select(x => x.ToString());
            switch (contentTypeHeader)
            {
                case "application/sparql-query":
                case "text/plain":
                    var reader = new StreamReader(context.Request.Body, enc);
                    return new SparqlQuery((await reader.ReadToEndAsync()), ctypes);
                case "application/json":
                    var json = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (json == null || json["query"] == null) { throw new InvalidOperationException("Missing SPARQL query"); }
                    return new SparqlQuery(json["query"], ctypes);
                case "application/x-www-form-urlencoded":
                case "multipart/form-data":
                    var form = await context.Request.ReadFormAsync();
                    var hasQuery = form.TryGetValue("query", out var sparql);
                    if (!hasQuery || !string.IsNullOrEmpty(sparql)) {throw new InvalidOperationException("Missing SPARQL query"); }
                    return new SparqlQuery(sparql.ToString(), ctypes);
                default:
                    throw new BadHttpRequestException($"{context.Request.ContentType} not supported for SPARQL request", 400);
            }
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// MVC wrapper for Minimal API 'BindAsync' pattern. Delete this class when porting to minimal api
    /// </summary>
    public class SparqlQueryInputFormatter : InputFormatter
    {
        public SparqlQueryInputFormatter()
        {
            foreach (var mime in SparqlQuery.SupportedMediaTypes) SupportedMediaTypes.Add(mime);
        }
        protected override bool CanReadType(Type type) => type == typeof(SparqlQuery);

        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var bound = await SparqlQuery.BindAsync(context.HttpContext);
            return await InputFormatterResult.SuccessAsync(bound);
        }
    }
}
