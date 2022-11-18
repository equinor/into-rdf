using Services.RevisionTrainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public static class SetupEndpoints
{
    private static readonly string[] trainTag = { "Revision trains" };

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        app.MapPost("revision-trains", [Authorize] async (HttpRequest request, HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.CreateRevisionTrain(request);
                SetContextContentType(context, response);
                return await response.Content.ReadAsStringAsync();
            })
            .Accepts<string>("text/turtle; charset=UTF-8")
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(trainTag);

        app.MapGet("revision-trains/{id}", [Authorize] async (string id, HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.GetRevisionTrain(id);
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

        app.MapDelete("revision-trains/{id}", [Authorize] async (string id, HttpContext context, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.DeleteRevisionTrain(id);
                SetContextContentType(context, response);
                return await response.Content.ReadAsStringAsync();
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