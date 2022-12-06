using Common.FusekiModels;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Services.FusekiMappers;

namespace Services.FusekiServices;

public class FusekiQueryService : IFusekiQueryService
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<FusekiQueryService> _logger;

    public FusekiQueryService(IFusekiService fusekiService, ILogger<FusekiQueryService> logger)
    {
        _fusekiService = fusekiService;
        _logger = logger;
    }

    public async Task<List<T>> Select<T>(string server, string sparql) where T : new()
    {
        var result = FusekiResponseToPropsMapper.MapResponse<T>(await Select(server, sparql));

        return result;
    }

    private async Task<FusekiSelectResponse> Select(string server, string sparql)
    {
        var result = await _fusekiService.Query(server, sparql);
        var serializedResult = await FusekiUtils.SerializeResponse(result);

        return FusekiUtils.DeserializeToFusekiSelectResponse(serializedResult);
    }

    public async Task<bool> Ask(string server, string query)
    {
        var askResponse = await _fusekiService.Query(server, query);
        var serializedResponse = await FusekiUtils.SerializeResponse(askResponse);
        var answer = FusekiUtils.DeserializeToFusekiAskResponse(serializedResponse);

        if (answer == null)
        {
            _logger.LogError($"Fuseki failed to create an Ask response");
            throw new InvalidOperationException($"Fuseki failed to create an Ask response");
        }

        return answer.Boolean;
    }
}