using Common.AppsettingsModels;
using Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Text;

namespace Services.FusekiService
{
    public class FusekiService : IFusekiService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public FusekiService(IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
        }

        public async Task<HttpResponseMessage> Post(string server, string turtle)
        {
            var request = await SetupRequest(server, "ds/data", "text/turtle");
            request.Content = new StringContent(turtle);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/turtle");
            var response = await CreateClient(server).SendAsync(request);
            return response;
        }

        public async Task<string> Query(string server, string sparql)
        {
            var request = await SetupRequest(server, "ds/query", "application/json");
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
            new("query", sparql)
            });
            var response = await CreateClient(server).SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (content.StartsWith("Parse error")) throw new FusekiException(content);
            return content;
        }

        private async Task<HttpRequestMessage> SetupRequest(string server, string endpoint, string contentType)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {await _tokenAcquisition.GetAccessTokenForUserAsync(new [] { GetScopes(server) })}");
            request.Headers.Add("Accept", contentType);
            request.Headers.Add("Encoding", Encoding.UTF8.WebName);
            return request;
        }

        private HttpClient CreateClient(string server) => _clientFactory.CreateClient(server.ToLower());

        private string GetScopes(string server)
        {
            var lowercaseServer = server.ToLower();
            var scopes = _configuration.GetSection("Servers").Get<List<RdfServer>>().Find(rdfServer => rdfServer.Name == lowercaseServer)?.Scopes;
            if (string.IsNullOrEmpty(scopes)) throw new ArgumentException("Invalid server name");
            return scopes;
        }
    }
}
