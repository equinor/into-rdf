using Common.Constants;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;

namespace Services.RdfServices
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly ILogger<RdfService> _logger;
        public RdfService(IFusekiService fusekiService,
                          ILogger<RdfService> logger)
        {
            _fusekiService = fusekiService;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query, IEnumerable<string?>? accepts = null)
        {
            return await _fusekiService.Query(server, query);
        }
    }
}
