using Common.Exceptions;
using Microsoft.Identity.Web;

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

        private async Task<HttpResponseMessage> ExecuteSparql(string server, string sparql, string contentType)
        {
            return await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options =>
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
