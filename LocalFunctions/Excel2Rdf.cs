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
using Newtonsoft.Json;
using Common.SpreadsheetModels;
using Services.RdfServices;
using Services.SpineNotificationServices;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LocalFunctions;

public class Excel2Rdf
{
    //private readonly BlobServiceClient _blobServiceClient;
    private readonly IRdfService _rdfService;

    private const string InputContainer = "spreadsheet-data";
    private const string OutputContainer = "transformed-data";

    public Excel2Rdf(IRdfService rdfService)
    {
        _rdfService = rdfService;
    }
    [FunctionName(nameof(Excel2Rdf))]
    public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
                            HttpRequest req, ILogger log)
    {
        var spreadsheetInfo = await GetSpreadsheetInfo(req);

        var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("TargetStorageAccount"));
        var blobInputClient = blobServiceClient.GetBlobContainerClient(InputContainer).GetBlobClient(spreadsheetInfo.FileName);
        var blobContent = await blobInputClient.DownloadContentAsync();
        
        log.LogInformation("Starting transformation of {file}", blobInputClient.Name);
        
        var turtle = await _rdfService.HandleSpreadsheetRequest(spreadsheetInfo, blobContent);

        if (turtle == String.Empty) return;

        var filename = spreadsheetInfo != null && spreadsheetInfo.FileName != null ? 
                        spreadsheetInfo.FileName[..spreadsheetInfo.FileName.LastIndexOf('.')] + ".ttl" :
                        throw new InvalidOperationException("Unable to generate output file name");
        var blobOutputClient = blobServiceClient.GetBlobContainerClient(OutputContainer).GetBlobClient(filename);
        await blobOutputClient.UploadAsync(BinaryData.FromString(turtle), overwrite: true);

        log.LogInformation("Successfully transformed {file}", blobInputClient.Name);
    }

    private async Task<SpreadsheetInfo> GetSpreadsheetInfo(HttpRequest req)
    {
        string requestBody = String.Empty;
        using (StreamReader streamReader = new StreamReader(req.Body))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }
        var info = JsonConvert.DeserializeObject<SpreadsheetInfo>(requestBody);
        ArgumentNullException.ThrowIfNull(info);
        if (!info.TryValidate(out var exception))
        {
            throw exception;
        }
        
        return info;
    }
}

