using Api.Utils.Bindings;
using Common.Exceptions;
using Common.GraphModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.GraphParserServices;
using Services.RecordServices;
using Services.RevisionServices;
using Services.Utils;
using VDS.RDF;

namespace Api;

public static class RouteBuilderExtensions
{
    private static readonly string[] trainTag = { "Revision trains" };

    public static B MapRevisionTrainEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
   {
        app.MapPost("revision-trains", [Authorize] async (HttpRequest request, HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.AddRevisionTrain(request);
                SetContextContentType(context, response);
                return await response.Content.ReadAsStringAsync();
            })
            .Accepts<string>("text/turtle; charset=UTF-8")
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        app.MapGet("revision-trains/{id}", [Authorize] async (string id, HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.GetRevisionTrainByName(id);
                SetContextContentType(context, response);
                return await response.Content.ReadAsStringAsync();
            })
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        app.MapGet("revision-trains/", [Authorize] async (HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.GetAllRevisionTrains();
                SetContextContentType(context, response);
                return await response.Content.ReadAsStringAsync();
            })
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        app.MapDelete("revision-trains/{id}", [Authorize] async (
            string id,
            HttpContext context,
            [FromServices] IRevisionTrainService revisionTrainService,
            [FromServices] IRecordService recordService,
            [FromServices] IGraphParser graphParser)
            =>
            {
                var train = await revisionTrainService.GetRevisionTrainByName(id);
                var revisionTrain = await train.Content.ReadAsStringAsync();
                var revisionTrainModel = graphParser.ParseRevisionTrain(revisionTrain);

                var records = revisionTrainModel.Records.Select(r => r.RecordUri).ToList();

                var responseTrain = await revisionTrainService.DeleteRevisionTrain(id);

                if (!responseTrain.IsSuccessStatusCode) { throw new InvalidOperationException($"Failed to delete revision train {revisionTrainModel.Name}"); }

                var responseRecord = await recordService.Delete(revisionTrainModel.TripleStore, records);

                if (!responseRecord.IsSuccessStatusCode)
                {
                    var restore = await revisionTrainService.RestoreRevisionTrain(revisionTrain);

                    var message = await responseRecord.Content.ReadAsStringAsync();
                    var restoreMessage = restore.IsSuccessStatusCode ? "WARNING: Revision train is successfully restored" : "ERROR: Failed to restore revision train";
                    throw new InvalidOperationException($"{restoreMessage}, failed to delete the train's records because {message}.");
                }

                SetContextContentType(context, responseTrain);
                return await responseTrain.Content.ReadAsStringAsync();
            })
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);
        return app;
    }
    private static readonly string[] namedGraphTag = { "Record" };
    
    private static readonly string[] recordTag = { "Record" };
    public static B MapRecordEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
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
    public static B MapAPIEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
    {
        app.MapRevisionTrainEndpoints();
        app.MapRecordEndpoints();
        app.MapControllers();
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
