using System.Net.Http.Headers;
using Common.AppsettingsModels;
using Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace Services.FusekiServices;

public class FusekiService : IFusekiService
{
    private readonly IDownstreamWebApi _downstreamWebApi;
    private readonly List<string> _fusekis; // only for debugging

    public FusekiService(IConfiguration config, IDownstreamWebApi downstreamWebApi)
    {
        _downstreamWebApi = downstreamWebApi;
        _fusekis = config.GetSection(ApiKeys.Servers)?.Get<List<RdfServer>>()?.Select(f => f.Name)?.ToList() ?? new List<string>();
    }

    public async Task<HttpResponseMessage> Query(string server, string sparql, IEnumerable<string?>? accepts = null)
    {
        accepts ??= new []{ "text/turtle", "application/sparql-results+json" };
        VerifyServer(server);
        var response = await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options 
            => GetDownStreamWebApiOptionsForQuery(options, sparql, accepts));

        await FusekiUtils.ValidateResponse(response);

        return response;
    }

    public async Task<HttpResponseMessage> Update(string server, string sparql)
    {
        VerifyServer(server);
        var response = await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options 
            => GetDownStreamWebApiOptionsForUpdate(options, sparql, new List<string> { "application/html"}));

        return response;
    }

    public async Task<HttpResponseMessage> AddData(string server, string graph, string contentType)
    {
        VerifyServer(server);
        var response = await _downstreamWebApi.CallWebApiForAppAsync(server.ToLower(), options
            => GetDownStreamWebApiOptionsForData(options, graph, contentType));

        return response;
    }

    private void VerifyServer(string server)
    {
        if (_fusekis.Count() == 0 || !_fusekis.Contains(server))
        {
            throw new Exception($"Downstream Fuskeki named {server} not found among [{string.Join(", ", _fusekis)}]");
        }
    }

    private DownstreamWebApiOptions GetDownStreamWebApiOptionsForUpdate(DownstreamWebApiOptions options, string sparql, List<string> accepts)
    {
        options.HttpMethod = HttpMethod.Post;
        options.RelativePath = "ds/update";
        options.CustomizeHttpRequestMessage = message =>
        {
            message.Headers.Add("Accept", accepts);
            message.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("update", sparql)
            });
        };

        return options;
    }

    private DownstreamWebApiOptions GetDownStreamWebApiOptionsForQuery(DownstreamWebApiOptions options, string sparql, IEnumerable<string?> accepts)
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

    private DownstreamWebApiOptions GetDownStreamWebApiOptionsForData(DownstreamWebApiOptions options, string graph, string contentType)
    {
        options.HttpMethod = HttpMethod.Post;
        options.RelativePath = "ds/data";
        options.CustomizeHttpRequestMessage = message =>
        {
            message.Headers.Add("Accept", contentType);
            message.Content = new StringContent(graph);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        };

        return options;
    }
}
