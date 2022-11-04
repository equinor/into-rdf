using Common.FusekiModels;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Services.FusekiServices;

public class FusekiAskService : IFusekiAskService
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<FusekiAskService> _logger;
    public FusekiAskService(IFusekiService fusekiService, ILogger<FusekiAskService> logger)
    {
        _fusekiService = fusekiService;
        _logger = logger; 
    }

    public async Task<bool> Ask(string server, string query)
    {
        var askResponse = await _fusekiService.QueryAsApp(server, query);
        var answer = JsonConvert.DeserializeObject<FusekiAskResponse>(askResponse);

        if (answer == null)
        {
            _logger.LogError($"Fuseki failed to create an Ask response");
            throw new InvalidOperationException($"Fuseki failed to create an Ask response");
        }

        return answer.Boolean;
    }
}