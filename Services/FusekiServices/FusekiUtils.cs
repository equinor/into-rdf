using Common.FusekiModels;
using Common.Exceptions;
using Newtonsoft.Json;

namespace Services.FusekiServices;

public static class FusekiUtils
{
    public static async Task<string> SerializeResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (content.StartsWith("Parse error")) throw new FusekiException(content);

        return content;
    }

    public static async Task ValidateResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (content.StartsWith("Parse error")) throw new FusekiException(content);
    }

    public static FusekiSelectResponse DeserializeToFusekiSelectResponse(string result)
    {
        var fusekiResponse = JsonConvert.DeserializeObject<FusekiSelectResponse>(result);

        if (fusekiResponse == null)
        {
            throw new InvalidOperationException("Failed to get response from Sparql query");
        }

        return fusekiResponse;
    }

    public static FusekiAskResponse DeserializeToFusekiAskResponse(string result)
    {
        var fusekiResponse = JsonConvert.DeserializeObject<FusekiAskResponse>(result);

        if (fusekiResponse == null)
        {
            throw new InvalidOperationException("Failed to get response from Sparql query");
        }

        return fusekiResponse;
    }
}
