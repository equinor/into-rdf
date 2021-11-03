using System;
using System.IO;
using Excel2ttl.Mel;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Mel2ttl
{
    public static class MelXslxToRDF
    {
        [Function("MelXslxToRDF")]
        public static void Run([BlobTrigger("melexcel/{name}", Connection = "dugtrioexperimental_STORAGE")] byte[] inputMel,
         string myBlob,
         string name,
        FunctionContext context)
        {
            var storageConnection = Environment.GetEnvironmentVariable("dugtrioexperimental_STORAGE");
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnection);
            BlobContainerClient logContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("logContainer"));
            var parselogBlob = logContainerClient.GetAppendBlobClient("ParseLog");
            parselogBlob.CreateIfNotExists();

            writeToParseLog($"Detected new file {name}", parselogBlob);

            if (name.EndsWith(".xlsx"))
            {
                writeToParseLog($"Starting parsing of {name}", parselogBlob);
                var logger = context.GetLogger("MelXslxToRDF");
                logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {myBlob}");

                string resString = string.Empty;

                using (var inStream = new MemoryStream(inputMel))
                {
                    resString = new Mel2TtlMapper(logger).Map(name, inStream);
                }
                if (resString != string.Empty)
                {
                    writeToParseLog($"Successfully parsed {name}", parselogBlob);
                    var strippedName = name.Replace("xlsx", "ttl");
                    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("sourceContainer"));
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
