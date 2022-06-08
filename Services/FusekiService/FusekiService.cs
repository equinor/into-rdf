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

        public async Task<HttpResponseMessage> Post(string server, string turtle)
        {
            var contentType = "text/turtle";
            return await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options =>
            {
                options.HttpMethod = HttpMethod.Post;
                options.RelativePath = "ds/data";
                options.CustomizeHttpRequestMessage = message =>
                {
                    message.Headers.Add("Accept", contentType);
                    message.Content = new StringContent(turtle);
                    message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                };
            });
        }

        public async Task<string> Query(string server, string sparql)
        {
            var response = await ExecuteSparql(server, sparql, "application/json");
            var content = await response.Content.ReadAsStringAsync();

            if (content.StartsWith("Parse error")) throw new FusekiException(content);
            
            return content;
        }

        public async Task<FusekiResponse> QueryFusekiServer(string server, string sparql)
        {
            var result = await Query(server, sparql);

            return JsonConvert.DeserializeObject<FusekiResponse>(result);
        }

        private async Task<HttpResponseMessage> ExecuteSparql(string server, string sparql, string contentType)
        {
            return await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options =>
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
            });
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