using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.TieModels;
using Doc2Rdf.Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiService;
using Services.ProvenanceService;
using Services.TieMessageService;

namespace Services.RdfService
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly IMelTransformer _melTransformer;
        private readonly IProvenanceService _provenanceService;
        private readonly ITieMessageService _tieMessageService;
        private readonly ILogger<RdfService> _logger;
        public RdfService(IFusekiService fusekiService,
                          IMelTransformer melTransformer,
                          IProvenanceService provenanceService,
                          ITieMessageService tieMessageService,
                          ILogger<RdfService> logger)
        {
            _fusekiService = fusekiService;
            _melTransformer = melTransformer;
            _provenanceService = provenanceService;
            _tieMessageService = tieMessageService;
            _logger = logger;
        }

        public async Task<string> ConvertDocToRdf(IFormFile formFile)
        {
            using var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            return _melTransformer.Transform(stream, formFile.FileName);
        }

        public async Task HandleStorageFiles(List<BlobDownloadResult> blobData)
        {
            TieData tieData = _tieMessageService.ParseXmlMessage(blobData);
            _logger.LogInformation("Parsed TIE message for {TieFileData}", tieData.FileData.Name);

            Provenance provenance = await _provenanceService.CreateProvenanceFromTieMessage(tieData);
            _logger.LogInformation("Created provenance information for facility '{FacilityId}' with revision name '{RevisionName}'",
                        provenance.FacilityId, provenance.RevisionName);

            //TODO - Update transform

            //TODO - Push transformed data to Fuseki
        }

        public async Task<HttpResponseMessage> PostToFuseki(string server, string data)
        {
            return await _fusekiService.Post(server, data);
        }

        public async Task<string> QueryFuseki(string server, string query)
        {
            return await _fusekiService.Query(server, query);
        }
    }
}
