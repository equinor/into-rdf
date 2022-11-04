using Services.RevisionTrainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.RevisionTrainModels;

public static class SetupEndpoints
{
    private static readonly string[] trainTag = {"Revision trains"};

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        app.MapPost("revision-trains", [Authorize] async (HttpRequest request, [FromServices] IRevisionTrainService revisionTrainService)
            => await revisionTrainService.CreateRevisionTrain(request))
            .Accepts<string>("text/turtle; charset=UTF-8")
            .Produces<string>(
                StatusCodes.Status200OK,
                "text/turtle"
            )
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(trainTag);
        
        app.MapGet("revision-trains/{id}", [Authorize] async (string id, [FromServices] IRevisionTrainService revisionTrainService)
            => await revisionTrainService.GetRevisionTrain(id))
            .Produces<RevisionTrainModel>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        app.MapGet("revision-trains/", [Authorize] async ([FromServices] IRevisionTrainService revisionTrainService)
            => await revisionTrainService.GetAllRevisionTrains())
            .Produces<List<RevisionTrainModel>>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        app.MapDelete("revision-trains/{id}", [Authorize] async (string id, [FromServices] IRevisionTrainService revisionTrainService)
            => await revisionTrainService.DeleteRevisionTrain(id))
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .WithTags(trainTag);

        return app;
    }
}
