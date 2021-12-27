using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Doc2Rdf.Library;

namespace Excel2Turtle.Functions.Xlst2Turtle
{
    public static class Xlst2Ttl
    {
        [FunctionName("XlstToTtl")]
        public static void Run([BlobTrigger("melexcel/{name}", Connection = "connection")] Stream inputMel, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputMel.Length} Bytes");
            var storageConnection = Environment.GetEnvironmentVariable("connection");
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnection);
            BlobContainerClient logContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("logContainer"));
            var parselogBlob = logContainerClient.GetAppendBlobClient("ParseLog");
            parselogBlob.CreateIfNotExists();

            writeToParseLog($"Detected new file {name}", parselogBlob);

            if (name.ToLower().EndsWith(".xlsx"))
            {
                writeToParseLog($"Starting parsing of {name}", parselogBlob);

                string resString = string.Empty;

                try
                {
                    var content = Doc2RdfTransformer.Initialize(name);
                    resString = Doc2RdfTransformer.Transform(content);
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex.Message);
                    log.LogInformation($"Error parsing {name} {ex.Message}");
                }

                if (resString != string.Empty)
                {
                    writeToParseLog($"Successfully parsed {name}", parselogBlob);
                    log.LogInformation($"Successfully parsed {name}");
                    var strippedName = name.Replace("xlsx", "ttl").Replace("XLSX", "ttl");
                    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("targetContainer"));
                    BlobClient blobClient = blobContainerClient.GetBlobClient(strippedName);
                    writeToParseLog($"Uploading {strippedName} to storage", parselogBlob);
                    using (var stream = new MemoryStream())
                    {
                        var writer = new StreamWriter(stream);
                        writer.Write(resString);
                        writer.Flush();
                        stream.Position = 0;
                        blobClient.Upload(stream);
                    }
                    writeToParseLog($"Successfully Uploaded {strippedName} to storage", parselogBlob);
                    log.LogInformation($"Successfully uploaded {name}");
                }
                else
                {
                    log.LogInformation($"No data extracted from {name}");
                }

            }
        }
        private static void writeToParseLog(string logEvent, AppendBlobClient blobClient)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write($"{DateTime.UtcNow} - {logEvent}{Environment.NewLine}");
            writer.Flush();
            stream.Position = 0;
            blobClient.AppendBlock(stream);
        }
    }
}
