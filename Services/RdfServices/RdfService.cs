using Common.Constants;
using Common.GraphModels;
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

        public async Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType)
        {
            return await _fusekiService.AddData(server, new ResultGraph(GraphConstants.Default, data), contentType);
        }

        public async Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query, IEnumerable<string?>? accepts = null)
        {
            return await _fusekiService.Query(server, query);
        }
    }
}
