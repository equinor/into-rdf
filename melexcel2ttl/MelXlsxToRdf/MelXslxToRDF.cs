using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Melexcel2ttl
{
    public static class MelXslxToRDF
    {
        [Function("MelXslxToRDF")]
        public static void Run([BlobTrigger("melexcel/{name}", Connection = "dugtrioexperimental_STORAGE")] System.ReadOnlyMemory<Byte> inputMel,
         string myBlob,
         string name,
        FunctionContext context)
        {
            if (name.EndsWith(".xlsx"))
            {
                var logger = context.GetLogger("MelXslxToRDF");
                logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {myBlob}");
                var inStream = new MemoryStream(inputMel.ToArray());
                var mapper = new Xslx2TtlMapper();
                String resString = mapper.Map(name, inStream);
                var storageConnection = Environment.GetEnvironmentVariable("dugtrioexperimental_STORAGE");
                BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnection);
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("melexcel");
                var strippedName = name.Replace("xslx", "");
                BlobClient blobClient = blobContainerClient.GetBlobClient($"{strippedName}.ttl");
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(resString);
                writer.Flush();
                stream.Position = 0;
                blobClient.Upload(stream);
            }
        }
    }
}
