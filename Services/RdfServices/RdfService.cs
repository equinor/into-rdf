
using Azure.Storage.Blobs.Models;
using Common.Constants;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
using Services.ProvenanceServices;
using Services.TieMessageServices;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.TransformationServices.XMLTransformationServices;
using System.Net;

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

        public async Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query)
        {
            return await _fusekiService.Query(server, query);
        }
    }
}
