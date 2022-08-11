using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Microsoft.AspNetCore.Http;

namespace Services.RdfServices
{
    public interface IRdfService
    {
        Task<string> ConvertDocToRdf(IFormFile formFile);
        Task<HttpResponseMessage> PostToFusekiAsUser(string server, string data, string contentType = "text/turtle");
        Task<string> QueryFusekiAsUser(string server, string query);
        Task<string> HandleSpreadsheetRequest(SpreadsheetInfo info, BlobDownloadResult blobData);
        Task<Provenance?> HandleTieRequest(string datasource, List<BlobDownloadResult> blobData);
    }
}