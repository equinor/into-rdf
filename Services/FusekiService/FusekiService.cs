using Common.Exceptions;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Text.Json;

namespace Services.FusekiService
{
    public class FusekiService : IFusekiService
    {
        private readonly IDownstreamWebApi _downstreamWebApi;

        public FusekiService(IDownstreamWebApi downstreamWebApi)
        {
            _downstreamWebApi = downstreamWebApi;
        }

        public async Task<HttpResponseMessage> PostAsApp(string server, string turtle)
        {
            return await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForData(options, turtle));
        }

        public async Task<HttpResponseMessage> PostAsUser(string server, string turtle)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForData(options, turtle));
        }

        public async Task<string> QueryAsApp(string server, string sparql)
        {
            var response = await ExecuteSparqlAsApp(server, sparql, "application/json");

            return await SerializeResponse(response);
        }

        public async Task<string> QueryAsUser(string server, string sparql)
        {
            var response = await ExecuteSparqlAsUser(server, sparql, "application/json");

            return await SerializeResponse(response);
        }

        public async Task<FusekiResponse> QueryFusekiResponseAsApp(string server, string sparql)
        {
            var result = await QueryAsApp(server, sparql);

            return DeserializeToFusekiResponse(result);
        }

        public async Task<FusekiResponse> QueryFusekiResponseAsUser(string server, string sparql)
        {
            var result = await QueryAsUser(server, sparql);
            
            return DeserializeToFusekiResponse(result);
        }

        private async Task<string> SerializeResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (content.StartsWith("Parse error")) throw new FusekiException(content);

            return content;
        }

        private FusekiResponse DeserializeToFusekiResponse(string result)
        {
            var fusekiResponse = JsonConvert.DeserializeObject<FusekiResponse>(result);

            if (fusekiResponse == null)
            {
                throw new InvalidOperationException("Failed to get response from Sparql query");
            }

            return fusekiResponse;
        }

        private async Task<HttpResponseMessage> ExecuteSparqlAsApp(string server, string sparql, string contentType)
        {
            return await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForQuery(options, sparql, contentType));
        }

        private async Task<HttpResponseMessage> ExecuteSparqlAsUser(string server, string sparql, string contentType)
        {
            return  await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForQuery(options, sparql, contentType));
        }

        private DownstreamWebApiOptions GetDownStreamWebApiOptionsForQuery(DownstreamWebApiOptions options, string sparql, string contentType)
        {
                options.HttpMethod = HttpMethod.Post;
                options.RelativePath = "ds/query";
                options.CustomizeHttpRequestMessage = message =>
                {
                    message.Headers.Add("Accept", contentType);
                    message.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                    {
                        new("query", sparql)
                    });
                };

            return options;
        }

        private DownstreamWebApiOptions GetDownStreamWebApiOptionsForData(DownstreamWebApiOptions options, string turtle)
        {
            var contentType = "text/turtle";

            options.HttpMethod = HttpMethod.Post;
            options.RelativePath = "ds/data";
            options.CustomizeHttpRequestMessage = message =>
            {
                message.Headers.Add("Accept", contentType);
                message.Content = new StringContent(turtle);
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            };

            return options;
        }
    }
}

public class FusekiResponse
{
    public FusekiHead Head { get; set; } = new();
    public FusekiResult Results { get; set; } = new();
}

public class FusekiHead
{
    public List<string> Vars { get; set; } = new();
}

public class FusekiResult
{
    public List<Dictionary<string, Triplet>> Bindings { get; set; } = new();
}

public class Triplet
{
    public string? Type { get; set; }
    public string? Datatype { get; set; }
    public string? Value { get; set; }
}