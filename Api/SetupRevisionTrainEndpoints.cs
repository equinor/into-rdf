using Services.GraphParserServices;
using Services.RecordServices;
using Services.RevisionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public static class SetupRevisionTrainEndpoints
{
    private static readonly string[] trainTag = { "Revision trains" };

    public static WebApplication AddRevisionTrainEndpoints(this WebApplication app)
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

    public static void SetContextContentType(HttpContext context, HttpResponseMessage response)
    {

        if (context != null && context.Response != null)
        {
            context.Response.ContentType = response.Content.Headers?.ContentType?.ToString() ?? "text/turtle";
        }
    }
}
