using System.Net.Http.Headers;
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
        Task<HttpResponseMessage> QueryFusekiAsUser(string server, string query);
        Task<string> HandleSpreadsheetRequest(string server, SpreadsheetInfo info, BlobDownloadResult blobData);
        Task<Provenance?> HandleTieRequest(string server, string datasource, List<BlobDownloadResult> blobData);
    }
}