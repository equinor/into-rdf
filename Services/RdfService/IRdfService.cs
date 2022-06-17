using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Services.RdfService
{
    public interface IRdfService
    {
        Task<string> ConvertDocToRdf(IFormFile formFile);
        Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data);
        Task<string> QueryFusekiAsUser(string server, string query);
        Task HandleStorageFiles(List<BlobDownloadResult> blobData);
    }
}