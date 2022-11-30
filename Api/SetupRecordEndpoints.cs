using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Utils.Bindings;
using Common.Exceptions;
using Common.GraphModels;
using Services.RecordServices;
using Services.RevisionServices;
using Services.GraphParserServices;
using Services.Utils;

public static class SetupNamedGraphsEndpoints
{
    private static readonly string[] namedGraphTag = { "Record" };

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

            //Get train
            var trainResponse = await revisionTrainService.GetRevisionTrain(revisionTrainName);
            var revisionTrain = await trainResponse.Content.ReadAsStringAsync();
            
            //Parse revision train
            var revisionTrainModel = graphParser.ParseRevisionTrain(revisionTrain);

            DateTime date = DateFormatter.FormateToDate(revisionDate);
            //Validate new revision
            revisionService.ValidateRevision(revisionTrainModel, revision, date);

            //Create new named graph entry
            ResultGraph recordContext = recordService.CreateRecordContext(revisionTrainModel, revision, date);
            
            switch (fileBinding.File.ContentType)
            {
                //Handle Excel
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    var response = await recordService.UploadExcel(revisionTrainModel, recordContext, fileBinding.File.OpenReadStream());
                    SetContextContentType(context, response);
                    return await response.Content.ReadAsStringAsync();

                //Handle AML - if recognized.
                case "application/AML":
                    //var rdf = await _xmlRdfService.ConvertAMLToRdf(formFile.OpenReadStream());
                    //var result = await _rdfService.PostToFusekiAsUser(server, rdf, "application/n-quads");
                    //return result.IsSuccessStatusCode ? Ok(rdf) : BadRequest(await result.Content.ReadAsStringAsync());
                    break;
                //text/turtle is currently not recognized by Swagger, but can be set for ordinary requests.
                case "text/turtle":
                    //using var streamReader = new StreamReader(formFile.OpenReadStream(), Encoding.UTF8);
                    //var content = await streamReader.ReadToEndAsync();
                    //var result = await _rdfService.PostToFusekiAsUser(server, content ?? string.Empty);
                    //return result.IsSuccessStatusCode ? Ok(content) : BadRequest(await result.Content.ReadAsStringAsync());
                    break;
                default:
                    throw new UnsupportedContentTypeException(@$"Unsupported Media Type for IFormFile {fileBinding.File.ContentType}.
                        Supported content types:
                            AML: application/AML,
                            Excel: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
                            RDF: text/turtle
                            ");
            }
            
            throw new InvalidOperationException("Unable to parse input");
        }
            )
            .Accepts<FileBinding>("multipart/form-data")
            .Produces<string>(
                StatusCodes.Status200OK
            )
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(namedGraphTag);

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