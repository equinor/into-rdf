using System.Net.Http.Headers;
using Common.Exceptions;
using Common.FusekiModels;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Services.FusekiMappers;

namespace Services.FusekiServices;

public class FusekiService : IFusekiService
{
    private readonly IDownstreamWebApi _downstreamWebApi;

    public FusekiService(IDownstreamWebApi downstreamWebApi)
    {
        _downstreamWebApi = downstreamWebApi;
    }

    public async Task<HttpResponseMessage> PostAsApp(string server, string turtle, string contentType = "text/turtle")
    {
        return await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForData(options, turtle, contentType));
    }

    public async Task<HttpResponseMessage> PostAsUser(string server, string turtle, string contentType = "text/turtle")
    {
        return await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForData(options, turtle, contentType));
    }

    public async Task<string> QueryAsApp(string server, string sparql)
    {
        var response = await ExecuteSparqlAsApp(server, sparql, new List<string> { "text/turtle", "application/sparql-results+json" });

        return await SerializeResponse(response);
    }

    public async Task<HttpResponseMessage> QueryAsUser(string server, string sparql)
    {
        return await ExecuteSparqlAsUser(server, sparql, new List<string> { "text/turtle", "application/sparql-results+json" });
    }

    public async Task<FusekiResponse> QueryFusekiResponseAsApp(string server, string sparql)
    {
        var result = await QueryAsApp(server, sparql);

        return DeserializeToFusekiResponse(result);
    }

    public async Task<List<T>> QueryFusekiResponseAsApp<T>(string server, string sparql) where T : new()
    {
        var result = FusekiResponseToPropsMapper.MapResponse<T>(await QueryFusekiResponseAsApp(server, sparql));

        return result;
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

    private async Task<HttpResponseMessage> ExecuteSparqlAsApp(string server, string sparql, List<string> accepts)
    {
        return await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForQuery(options, sparql, accepts));
    }

    private async Task<HttpResponseMessage> ExecuteSparqlAsUser(string server, string sparql, List<string> accepts)
    {
        return await _downstreamWebApi.CallWebApiForUserAsync(server.ToLower(), options => GetDownStreamWebApiOptionsForQuery(options, sparql, accepts));
    }

    private DownstreamWebApiOptions GetDownStreamWebApiOptionsForQuery(DownstreamWebApiOptions options, string sparql, List<string> accepts)
    {
        options.HttpMethod = HttpMethod.Post;
        options.RelativePath = "ds/query";
        options.CustomizeHttpRequestMessage = message =>
        {
            message.Headers.Add("Accept", accepts);
            message.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("query", sparql)
            });
        };

        return options;
    }

    private DownstreamWebApiOptions GetDownStreamWebApiOptionsForData(DownstreamWebApiOptions options, string turtle, string contentType)
    {

        options.HttpMethod = HttpMethod.Post;
        options.RelativePath = "ds/data";
        options.CustomizeHttpRequestMessage = message =>
        {
            message.Headers.Add("Accept", contentType);
            message.Content = new StringContent(turtle);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        };

        return options;
    }
}
