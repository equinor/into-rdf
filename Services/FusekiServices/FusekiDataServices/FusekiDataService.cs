using Microsoft.Extensions.Logging;

namespace Services.FusekiServices;

public class FusekiDataService
{
    private readonly IFusekiService _fusekiService;
    private readonly ILogger<FusekiDataService> _logger;

    public FusekiDataService(IFusekiService fusekiService, ILogger<FusekiDataService> logger)
    {
        _fusekiService = fusekiService;
        _logger = logger;
    }
}