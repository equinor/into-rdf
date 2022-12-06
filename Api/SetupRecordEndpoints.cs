using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Utils.Bindings;
using Common.Exceptions;
using Common.GraphModels;
using Services.RecordServices;
using Services.RevisionServices;
using Services.GraphParserServices;
using Services.Utils;
using VDS.RDF;

public static class SetupNamedGraphsEndpoints
{
    private static readonly string[] recordTag = { "Record" };

    public static WebApplication AddRecordEndpoints(this WebApplication app)
    {
        app.MapPost("record", [Authorize] async (
            string revisionTrainName,
            string revision,
            string revisionDate,
            HttpContext context,
            FileBinding fileBinding,
            [FromServices] IRecordService recordService,
            [FromServices] IRevisionTrainService revisionTrainService,
            [FromServices] IGraphParser graphParser,
            [FromServices] IRevisionService revisionService)
            =>
        {
            if (fileBinding.File is null) throw new InvalidOperationException("No file");

            var trainResponse = await revisionTrainService.GetRevisionTrainByName(revisionTrainName);
            var revisionTrain = await trainResponse.Content.ReadAsStringAsync();
            var revisionTrainModel = graphParser.ParseRevisionTrain(revisionTrain);
            DateTime date = DateFormatter.FormateToDate(revisionDate);

            revisionService.ValidateRevision(revisionTrainModel, revision, date);

            var transformed = String.Empty;

            switch (fileBinding.File.ContentType)
            {
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    transformed = recordService.TransformExcel(revisionTrainModel, fileBinding.File.OpenReadStream());
                    break;
                case "application/AML":
                    throw new NotImplementedException("Splinter will soon have AML support");
                case "text/turtle":
                    throw new NotImplementedException("WHAT? Isn't Splinter handling RDF yet? Ehhh, no");
                default:
                    throw new UnsupportedContentTypeException(@$"Unsupported Media Type for IFormFile {fileBinding.File.ContentType}.
                        Supported content types:
                            AML: application/AML,
                            Excel: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
                            RDF: text/turtle
                            ");
            }

            var recordContext = revisionTrainService.CreateRecordContext(revisionTrainModel, revision, date);
            var contextResponse = await revisionTrainService.AddRecordContext(recordContext);

            if (!contextResponse.IsSuccessStatusCode) { throw new InvalidOperationException($"Failed to add record context {recordContext.Content}"); }
            
            var recordResponse = await recordService.Add(revisionTrainModel.TripleStore, new ResultGraph(recordContext.Name, transformed));

            if (!recordResponse.IsSuccessStatusCode)
            {
                var restore = await revisionTrainService.DeleteRecordContext(new Uri(recordContext.Name));

                var message = await recordResponse.Content.ReadAsStringAsync();
                var restoreMessage = restore.IsSuccessStatusCode ? "WARNING: Record context is successfully restored" : "ERROR: Failed to restore record context";
                throw new InvalidOperationException($"{restoreMessage}, but failed to add record {recordContext.Name} because {message}.");
            }

            SetContextContentType(context, recordResponse);
            return await recordResponse.Content.ReadAsStringAsync();
        }
            )
            .Accepts<FileBinding>("multipart/form-data")
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(recordTag);

        app.MapDelete("record", [Authorize] async (
            string record,
            HttpContext context,
            [FromServices] IRecordService recordService,
            [FromServices] IRevisionTrainService revisionTrainService,
            [FromServices] IGraphParser graphParser,
            [FromServices] IRevisionService revisionService)
            =>
        {
            var recordUri = new Uri(record);
            var trainResponse = await revisionTrainService.GetRevisionTrainByRecord(recordUri);
            var revisionTrain = await trainResponse.Content.ReadAsStringAsync();
            var revisionTrainModel = graphParser.ParseRevisionTrain(revisionTrain);

            var latestRecord = revisionTrainModel.Records.MaxBy(rec => rec.RevisionNumber);

            if (latestRecord == null) { throw new ObjectNotFoundException($"Failed to delete record {recordUri.AbsoluteUri} because it doesn't exist."); }

            if (latestRecord.RecordUri != recordUri) { throw new InvalidOperationException($"Failed to delete record {recordUri.AbsoluteUri}. The latest revision {latestRecord.RecordUri} must be deleted first"); }

            var responseTrain = await revisionTrainService.DeleteRecordContext(latestRecord.RecordUri);

            if (!responseTrain.IsSuccessStatusCode){ throw new InvalidOperationException($"Failed to delete record {recordUri.AbsoluteUri}, because the record context couldn't be removed");}

            var responseRecord = await recordService.Delete(revisionTrainModel.TripleStore, latestRecord.RecordUri);

            if (!responseRecord.IsSuccessStatusCode)
            {
                var restore = await revisionTrainService.AddRecordContext(new ResultGraph(recordUri.AbsoluteUri, revisionTrain, true));

                var message = await responseRecord.Content.ReadAsStringAsync();
                var restoreMessage = restore.IsSuccessStatusCode ? "WARNING: Record context is successfully restored" : "ERROR: Failed to restore record context";
                throw new InvalidOperationException($"{restoreMessage}, but failed to delete record {record} because {message}.");
            }

            return await responseRecord.Content.ReadAsStringAsync();
        })
         .Produces<string>(StatusCodes.Status200OK, "application/html")
         .WithTags(recordTag);
        return app;
    }

    private static void SetContextContentType(HttpContext context, HttpResponseMessage response)
    {
        if (context != null && context.Response != null)
        {
            context.Response.ContentType = response.Content.Headers?.ContentType?.ToString() ?? "text/turtle";
        }
    }
}