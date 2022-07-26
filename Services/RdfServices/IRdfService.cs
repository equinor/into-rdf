using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Microsoft.AspNetCore.Http;

namespace Services.RdfServices
{
    public interface IRdfService
    {
        Task<string> ConvertDocToRdf(IFormFile formFile);
        Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data);
        Task<string> QueryFusekiAsUser(string server, string query);
        Task<Provenance?> HandleStorageFiles(List<BlobDownloadResult> blobData);
    }
}