using Common.Utils;
using Microsoft.Identity.Web;

namespace Services.CommonlibServices;

public sealed class CommonlibService : ICommonLibService
{
    private readonly IDownstreamWebApi _downstreamWebApi;


    public CommonlibService(IDownstreamWebApi downstreamWebApi)
    {
        _downstreamWebApi = downstreamWebApi;
    }

    public async Task<List<Dictionary<string, object>>> GetFromScopedLibrary(string library, string scope)
    {
        return await Query<Dictionary<string, object>>(
            $"FROM {library} " +
            $"WHERE IsValid = true and Scope = {scope}"
        );
    }

    private async Task<List<T>> Query<T>(string query)
    {
        var result = (await _downstreamWebApi
            .PostForUserAsync<List<T>, string>(
                ApiKeys.CommonLib,
                "GenericViews/query/text",
                query,
                options =>
                    options.CustomizeHttpRequestMessage = message =>
                        message.Content = new StringContent(query)
            )
        ) ?? new List<T>();
        return result;
    }
}
