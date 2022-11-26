using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.ValidationServices.RevisionTrainValidationServices;
using Common.Exceptions;
using Common.GraphModels;
using Common.Constants;
using Common.Utils;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Services.RevisionServices;

public class RevisionTrainService : IRevisionTrainService
{
    private readonly IFusekiService _fusekiService;
    private readonly IFusekiQueryService _fusekiQueryService;
    private readonly IRevisionTrainValidator _revisionTrainValidator;
    private readonly ILogger<RevisionTrainService> _logger;
    private readonly string _server;

    public RevisionTrainService(
        IFusekiQueryService fusekiQueryService,
        IFusekiService fusekiService,
        IRevisionTrainValidator revisionTrainValidator,
        ILogger<RevisionTrainService> logger)
    {
        _fusekiQueryService = fusekiQueryService;
        _fusekiService = fusekiService;
        _revisionTrainValidator = revisionTrainValidator;
        _logger = logger;
        _server = ServerKeys.Main;
    }

    public async Task<HttpResponseMessage> CreateRevisionTrain(HttpRequest request)
    {
        string revisionTrain;
        using (var stream = new StreamReader(request.Body))
        {
            revisionTrain = await stream.ReadToEndAsync();
        }

        if (revisionTrain == string.Empty)
        {
            _logger.LogWarning("Failed to create revision train because the train is empty");
            throw new InvalidOperationException("Failed to create revision train because the train is empty");
        }

        var validationReport = _revisionTrainValidator.ValidateRevisionTrain(revisionTrain);
        if (!validationReport.Conforms)
        {
            var report = String.Join(" ", validationReport.Results.Select(result => result.Message));
            _logger.LogWarning($"Failed to create revision train because the train definition is invalid: {report}");
            throw new ShapeValidationException($"Failed to create revision train because the train definition is invalid: {report}");
        }

        var trainExist = await RevisionTrainExist(revisionTrain);
        if (trainExist)
        {
            _logger.LogWarning($"Failed to upload revision train as a similarly named train already exists");
            throw new ConflictOnInsertException($"Failed to upload revision train as a similarly named train already exists");
        }

        var response = await _fusekiService.AddData(_server, new ResultGraph(GraphConstants.Default, revisionTrain), "text/turtle");
        _logger.LogInformation("Successfully uploaded revision train");

        return response;
    }

    public async Task<HttpResponseMessage> GetRevisionTrain(string name)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            _logger.LogWarning($"Failed to get train with name {name} because it doesn't exist");
            throw new FileNotFoundException($"Failed to get train with name {name} because it doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainQuery(name);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        return response;
    }

    public async Task<HttpResponseMessage> GetAllRevisionTrains()
    {
        var revisionTrainQuery = GetAllRevisionTrainQuery();
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        return response;
    }

    public async Task<HttpResponseMessage> DeleteRevisionTrain(string name)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            _logger.LogWarning($"Failed to delete train because a train with name {name} doesn't exist");
            throw new FileNotFoundException($"Failed to delete train because a train with name {name} doesn't exist");
        }

        var response = await _fusekiService.Update(_server, GetDeleteRevisionTrain(name));

        return response;
    }

    public async Task<Graph> GetRevisionTrainGraph(string revisionTrainGraph)
    {
        Graph revisionTrain = new Graph();
        var response = await GetRevisionTrain(revisionTrainGraph);
        revisionTrain.LoadFromString(await response.Content.ReadAsStringAsync(), new TurtleParser());
        return revisionTrain;
    }

    private async Task<bool> RevisionTrainExist(string revisionTrain)
    {
        var trainGraph = new Graph();
        trainGraph.LoadFromString(revisionTrain);
        var trainIri = trainGraph.GetTriplesWithObject(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#RevisionTrain"))).Single();
        var trainName = trainGraph.GetTriplesWithPredicate(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#name"))).Single();

        var trainExist = await _fusekiQueryService.Ask(_server, GetAskQuery(TripleContent.Subject, trainIri.Subject.ToString()));
        var trainNameExist = await _fusekiQueryService.Ask(_server, GetAskQuery(TripleContent.Object, trainName.Object.ToString()));

        if (trainExist || trainNameExist)
        {
            var message = trainExist 
                ? $"Failed to create new train because a train individual with IRI {trainIri.Subject.ToString()} already exists"
                : $"Failed to create new train because a train with name {trainName.Object.ToString()} already exists";
            _logger.LogWarning(message);
            return true;
        }

        return false;
    }

    private string GetRevisionTrainQuery(string name)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            ?train a splinter:RevisionTrain ;
                splinter:name '{name}' ;
                ?trainProp ?trainValue .
            OPTIONAL {{?trainValue ?contextProp ?contextValue .}} 
        }}
        ";

        return query;
    }

    private string GetAllRevisionTrainQuery()
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            ?train a splinter:RevisionTrain ;
                ?trainProp ?trainValue .
            OPTIONAL {{?trainValue ?contextProp ?contextValue .}} 
        }}
        ";

        return query;
    }

    private string GetAskQuery(TripleContent tripleContent, string name)
    {
        var pattern = string.Empty;
        switch (tripleContent)
        {
            case TripleContent.Subject:
                pattern = $"<{name}> ?p ?o .";
                break;
            case TripleContent.Predicate:
                pattern = $"?s <{name}> ?o .";
                break;
            case TripleContent.Object:
                pattern = $"?s ?p '{name}' .";
                break;
            default:
                _logger.LogWarning($"Failed to create Ask pattern for train {name}");
                break;
        }

        var query = 
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>
        ASK 
        {{
            {pattern}
        }}";

        return query;
    }

    private string GetDeleteRevisionTrain(string name)
    {
        var query = 
        @$"
        prefix commonlib: <https://rdf.equinor.com/commonlib/tie#>
        prefix splinter: <https://rdf.equinor.com/splinter#>
        prefix spreadsheet: <https://rdf.equinor.com/splinter/spreadsheet#>

        DELETE
        {{
            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        WHERE
        {{
            ?train splinter:name '{name}' ;
                (splinter:hasTieContext | splinter:hasSpreadsheetContext) ?context .

            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        ";

        return query;
    }
}