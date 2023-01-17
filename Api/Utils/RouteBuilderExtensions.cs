using Api.Utils.Bindings;
using Common.RevisionTrainModels;
using Common.TransformationModels;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Controller.RecordController;
using Repositories.OntologyRepository;
using Services.TransformerServices;
using Services.RevisionServices;
using System.Text;

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
                var revisionTrain = await ReadStreamAsync(request.Body);
                var response = await revisionTrainService.Add(revisionTrain);
                return Results.Created(context.Request.Path, response);
            })
            .Accepts<string>("text/turtle; charset=UTF-8")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(trainTag);

        app.MapGet("revision-trains/{id}", [Authorize] async (string id, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.GetByName(id);
                return Results.Text(response, "text/turtle", Encoding.UTF8);
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(trainTag);

        app.MapGet("revision-trains/", [Authorize] async ([FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                var response = await revisionTrainService.GetAll();
                return Results.Text(response, "text/turtle", Encoding.UTF8);
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(trainTag);

        app.MapDelete("revision-trains/", [Authorize] async (string name, [FromServices] IRevisionTrainService revisionTrainService)
            =>
            {
                await revisionTrainService.Delete(name);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(trainTag);
        return app;
    }

    private static readonly string[] recordTag = { "Record" };
    public static B MapRecordEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
    {
        app.MapPost("record", [Authorize] async (
            string revisionTrainName,
            string revision,
            string revisionDate,
            HttpContext context,
            TransformationBinding fileBinding,
            [FromServices] IRecordController recordService)
            =>
            {
                if (fileBinding.File is null) throw new InvalidOperationException("No file");

                var recordInput = new RecordInputModel(
                    revisionTrainName,
                    revision,
                    revisionDate,
                    fileBinding.File.OpenReadStream(),
                    fileBinding.File.ContentType);

                var response = await recordService.Add(recordInput);

                return Results.Created(context.Request.Path, response);
            })
            .Accepts<TransformationBinding>("multipart/form-data")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(recordTag);

        app.MapDelete("record", [Authorize] async (string record, [FromServices] IRecordController recordService)
            =>
            {
                await recordService.Delete(new Uri(record));
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(recordTag);

        return app;
    }

    private static readonly string[] transformationTag = { "Rdf transformation" };
    public static B MapTransformationEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
    {
        app.MapPost("transform/spreadsheet", [Authorize] async (
            TransformationBinding transformationBinding,
            [FromServices] ITransformerService transformerService,
            [FromServices] IOntologyRepository ontologyRepository)
            =>
            {
                if (transformationBinding.File is null) throw new InvalidOperationException("No file");
                if (transformationBinding.Details is null) throw new InvalidOperationException("Missing transformation details");


                var ontology = await GetOntology(transformationBinding.Details, ontologyRepository);

                var rdfGraph = transformerService.TransformSpreadsheet(transformationBinding.Details, transformationBinding.File.OpenReadStream());
                var enrichedGraph = transformerService.EnrichRdf(ontology, rdfGraph);
                var protoRecord = transformerService.CreateProtoRecord(transformationBinding.Details.Record, enrichedGraph);
                
                return Results.Text(protoRecord, "application/ld+json", Encoding.UTF8);
            })

            .Accepts<TransformationBinding>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(transformationTag);

        return app;
    }

    public static B MapAPIEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
    {
        app.MapRevisionTrainEndpoints();
        app.MapRecordEndpoints();
        app.MapTransformationEndpoints();
        app.MapControllers();
        return app;
    }

    private async static Task<string> ReadStreamAsync(Stream content)
    {
        var result = string.Empty;
        using (var streamReader = new StreamReader(content))
        {
            result = await streamReader.ReadToEndAsync();
        }
        return result;
    }

    private async static Task<string> GetOntology(SpreadsheetTransformationDetails transformationSettings, IOntologyRepository ontologyRepository)
    {
        if (transformationSettings.Level == EnrichmentLevel.None || transformationSettings.TransformationType == null)
        {
            return string.Empty;
        }

        var ontology = await ontologyRepository.Get(ServerKeys.Main, transformationSettings.TransformationType);
        return GraphSupportFunctions.WriteGraphToString(ontology, RdfWriterType.Turtle);
    }
}
