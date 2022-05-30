using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace Services.RdfService
{
    public interface IRdfService
    {
        Task<string> ConvertDocToRdf(IFormFile formFile);
        Task<HttpResponseMessage> PostToFuseki(string server, string data);
        Task<string> QueryFuseki(string server, string query);
        Task HandleStorageFiles(List<BlobDownloadResult> blobData);
    }
}