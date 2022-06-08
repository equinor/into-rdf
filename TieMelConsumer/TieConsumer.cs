using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Services.RdfService;

namespace TieMelConsumer
{
    public class TieConsumer
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IRdfService _rdfService;
        private const string container = "prodmeladapterfiles";
        public TieConsumer(BlobServiceClient blobServiceClient, IRdfService rdfService)
        {
            _blobServiceClient = blobServiceClient;
            _rdfService = rdfService;
        }
        [FunctionName(nameof(TieConsumer))]
        public async Task Run([BlobTrigger("prodmeladapterfiles/{name}")] Stream? myBlob, string name, ILogger log)
        {
            log.LogInformation("C# Blob trigger function Processed blob\n Name:{Name} \n Size: {Size}", name, myBlob?.Length);
            var blobs = await GetBlobsInSameDirectory(name);
            if (blobs.Count <= 1) return;

            _rdfService.HandleStorageFiles(await GetData(blobs));
        }

        private async Task<List<BlobItem>> GetBlobsInSameDirectory(string name)
        {
            var prefix = string.Join("/", name.Split("/").SkipLast(1));
            var blobs = new List<BlobItem>();
            await foreach (var blob in GetClient(container).GetBlobsAsync(prefix: prefix))
                blobs.Add(blob);
            return blobs;
        }

        private async Task<List<BlobDownloadResult>> GetData(IList<BlobItem> blobs)
        {
            var downloads = blobs.Select(blob => GetClient(container).GetBlobClient(blob.Name).DownloadContentAsync());
            var data = new List<BlobDownloadResult>();
            for (int i = 0; i < blobs.Count; i++)
            {
                var downloaded = await downloads.ElementAt(i);
                downloaded.Value.Details.Metadata.Add("Name", blobs[i].Name);
                data.Add(downloaded.Value);
            }
            return data;
        }

        private BlobContainerClient GetClient(string container) => _blobServiceClient.GetBlobContainerClient(container.ToLower());
    }
}
