using Doc2Rdf.Library;
using Microsoft.AspNetCore.Http;
using Services.FusekiService;
using Doc2Rdf.Library.Interfaces;
using Azure.Storage.Blobs.Models;

namespace Services.RdfService
{
    public class RdfService : IRdfService
    {
        private readonly IFusekiService _fusekiService;
        private readonly IMelTransformer _melTransformer;

        public RdfService(IFusekiService fusekiService, IMelTransformer melTransformer)
        {
            _fusekiService = fusekiService;
            _melTransformer = melTransformer;
        }

        public async Task<string> ConvertDocToRdf(IFormFile formFile)
        {
            using var stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            return _melTransformer.Transform(stream, formFile.FileName);
        }

        public Task HandleStorageFiles(List<BlobDownloadResult> blobData)
        {
            // Do work
            return Task.CompletedTask;
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
