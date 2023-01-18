using Api.Utils.Bindings;
using Common.TransformationModels;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.OntologyRepository;
using Services.TransformerServices;
using Services.RecordServices;
using System.Text;

namespace Api;

public static class RouteBuilderExtensions
{
    private static readonly string[] recordTag = { "Record" };
    public static B MapRecordEndpoints<B>(this B app)
    where B : IEndpointRouteBuilder
    {
        app.MapPost("record", [Authorize] async (
            HttpContext context,
            RecordBinding recordBinding,
            [FromServices] IRecordService recordService)
            =>
            {
                string[] validContentTypes = new string[]{"application/ld+json", "application/trig"};
                if (recordBinding is null) { throw new InvalidOperationException("Unable to bind input"); }
                if (recordBinding.Record is null) { throw new InvalidOperationException("No record to assert"); }
                if (validContentTypes.Contains(recordBinding.Record.ContentType))
                {
                    var contentTypes = validContentTypes.ToString();
                    throw new InvalidOperationException($"Wrong content type {recordBinding.Record.ContentType}. Expected {validContentTypes.ToString()}");
                }

                //Not working with Swagger as record content type become application/octet-stream
                await recordService.Add(recordBinding.Cursor, recordBinding.Record.OpenReadStream(), "application/ld+json");

                return Results.Ok();
            })
            .Accepts<RecordBinding>("multipart/form-data")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status502BadGateway)
            .WithTags(recordTag);

        app.MapDelete("record", [Authorize] async (string recordId, [FromServices] IRecordService recordService)
            =>
            {
                await recordService.Delete(new Uri(recordId));
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
