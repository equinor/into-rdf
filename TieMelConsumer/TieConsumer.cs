using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.AppsettingsModels;
using Common.ProvenanceModels;
using Common.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.RdfServices;
using Services.SpineNotificationServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TieMelConsumer;

public class TieConsumer
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IRdfService _rdfService;
    private readonly ISpineNotificationServices _spineNotificationService;
    private readonly IOptions<ServiceBusConfig> _serviceBusConfig;
    private const string container = "prodmeladapterfiles";

    public TieConsumer(BlobServiceClient blobServiceClient, IRdfService rdfService, ISpineNotificationServices spineNotificationService, IOptions<ServiceBusConfig> serviceBusConfig)
    {
        _blobServiceClient = blobServiceClient;
        _rdfService = rdfService;
        _spineNotificationService = spineNotificationService;
        _serviceBusConfig = serviceBusConfig;
    }

    //TODO - Rewrite functions to use Splinter API endpoints.
    [FunctionName(nameof(TieConsumer))]
    [StorageAccount("TieMelAdapterStorage")]
    public async Task Run([BlobTrigger("prodmeladapterfiles/{name}")] Stream? myBlob, string name, ILogger log)
    {
        var server = ServerKeys.OlDugtrio;
        log.LogInformation("C# Blob trigger function Processed blob\n Name:{Name} \n Size: {Size}", name, myBlob?.Length);

        var blobs = await GetBlobsInSameDirectory(name);
        if (blobs.Count <= 1) return;

        var provenance = await _rdfService.HandleTieRequest(server, DataSource.Mel, await GetData(blobs));

        if (provenance is null) return;
        if (string.IsNullOrEmpty(provenance.FacilityId) || string.IsNullOrEmpty(provenance.DocumentProjectId)) return;

        //await _spineNotificationService
        //   .PostToTopic(
        //       _serviceBusConfig.Value.Topic,
        //       new NewMelProcessedPayload(provenance.FacilityId, provenance.DocumentProjectId)
        //   );
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

public record NewMelProcessedPayload(string Facility, string DocumentProjectId);