using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.ValidationServices.RevisionTrainValidationServices;
using Common.GraphModels;
using Common.Constants;
using Common.RevisionTrainModels;
using Common.Utils;
using VDS.RDF;

namespace Services.RevisionTrainServices;

public class RevisionTrainService : IRevisionTrainService
{
    private readonly IFusekiService _fusekiService;
    private readonly IFusekiAskService _fusekiAskService;
    private readonly IRevisionTrainValidator _revisionTrainValidator;
    private readonly ILogger<RevisionTrainService> _logger;
    private readonly string _server;

    public RevisionTrainService(
        IFusekiAskService fusekiAskService,
        IFusekiService fusekiService,
        IRevisionTrainValidator revisionTrainValidator,
        ILogger<RevisionTrainService> logger)
    {
        _fusekiAskService = fusekiAskService;
        _fusekiService = fusekiService;
        _revisionTrainValidator = revisionTrainValidator;
        _logger = logger;
        _server = ServerKeys.Main;
    }

    public async Task<IResult> CreateRevisionTrain(HttpRequest request)
    {
        string revisionTrain;
        using (var stream = new StreamReader(request.Body))
        {
            revisionTrain = await stream.ReadToEndAsync();
        }

        if (revisionTrain == string.Empty)
        {
            _logger.LogWarning("Failed to create revision train because the train is empty");
            return Results.BadRequest("Failed to create revision train because the train is empty");
        }

        var validationReport = _revisionTrainValidator.ValidateRevisionTrain(revisionTrain);
        if (!validationReport.Conforms)
        {
            var report = String.Join(" ", validationReport.Results.Select(result => result.Message));
            _logger.LogWarning($"Failed to create revision train because the train definition is invalid: {report}");
            return Results.BadRequest($"Failed to create revision train because the train definition is invalid: {report}");
        }

        var trainExist = await RevisionTrainExist(revisionTrain);
        if (trainExist)
        {
            return Results.Conflict($"Failed to upload revision train as a similarly named train already exists");
        }

        var response = await _fusekiService.PostAsApp(_server, new ResultGraph(GraphConstants.Default, revisionTrain));
        _logger.LogInformation("Successfully uploaded revision train");

        return response.IsSuccessStatusCode ? Results.Text(revisionTrain) : Results.Problem(response.Content.ToString());
    }

    public async Task<IResult> GetRevisionTrain(string name)
    {
        var revisionTrainQuery = GetRevisionTrainQuery(name);
        var response = await _fusekiService.QueryFusekiResponseAsApp<RevisionTrainModel>(_server, revisionTrainQuery);

        if (response.Count == 0)
        {
            return Results.BadRequest($"No revision train with name {name} exists");
        }

        return response.Count == 1 ?
            Results.Ok(response.Single()) :
            Results.BadRequest($"{response.Count} revision trains returned, expected 1.");
    }

    public async Task<IResult> GetAllRevisionTrains()
    {
        var revisionTrainQuery = GetAllRevisionTrainQuery();
        var response = await _fusekiService.QueryFusekiResponseAsApp<RevisionTrainModel>(_server, revisionTrainQuery);

        return Results.Ok(response);
    }

    public async Task<IResult> DeleteRevisionTrain(string name)
    {
        var trainExist = await _fusekiAskService.Ask(_server, GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            _logger.LogWarning($"Failed to delete train because a train with name {name} doesn't exist");
            return Results.BadRequest($"Failed to delete train because a train with name {name} doesn't exist");
        }

        var response = await _fusekiService.UpdateAsApp(_server, GetDeleteRevisionTrain(name));

        return response.IsSuccessStatusCode ? Results.Ok($"Successfully deleted revision train with name {name}") : Results.BadRequest($"Failed to delete revision train with name {name}");
    }

    private async Task<bool> RevisionTrainExist(string revisionTrain)
    {
        var trainGraph = new Graph();
        trainGraph.LoadFromString(revisionTrain);
        var trainIri = trainGraph.GetTriplesWithObject(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#RevisionTrain"))).Single();
        var trainName = trainGraph.GetTriplesWithPredicate(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#name"))).Single();

        var trainExist = await _fusekiAskService.Ask(_server, GetAskQuery(TripleContent.Subject, trainIri.Subject.ToString()));
        var trainNameExist = await _fusekiAskService.Ask(_server, GetAskQuery(TripleContent.Object, trainName.Object.ToString()));

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
        prefix commonlib: <https://rdf.equinor.com/commonlib/tie#>
        prefix splinter: <https://rdf.equinor.com/splinter#>
        prefix spreadsheet: <https://rdf.equinor.com/splinter/spreadsheet#>

        SELECT ?DocumentName ?FacilityName ?ProjectCode ?ContractNumber ?TripleStore
        WHERE
        {{
            ?Train 
                a splinter:RevisionTrain ;
                splinter:name ?DocumentName ;
                splinter:triplestore ?TripleStore ;
                splinter:hasTieContext ?TieContext .
            
            ?TieContext
                a splinter:TieContext ;
                commonlib:facilityName ?FacilityName ;
                commonlib:projectCode ?ProjectCode ;
                commonlib:contractNumber ?ContractNumber .

            FILTER(?DocumentName = '{name}')
        }}
        ";

        return query;
    }

    private string GetAllRevisionTrainQuery()
    {
        var query =
        @$"
        prefix commonlib: <https://rdf.equinor.com/commonlib/tie#>
        prefix splinter: <https://rdf.equinor.com/splinter#>
        prefix spreadsheet: <https://rdf.equinor.com/splinter/spreadsheet#>

        SELECT DISTINCT ?DocumentName ?FacilityName ?ProjectCode ?ContractNumber ?TripleStore
        WHERE
        {{
            ?Train 
                a splinter:RevisionTrain ;
                splinter:name ?DocumentName ;
                splinter:triplestore ?TripleStore ;
                splinter:hasTieContext ?TieContext .
            
            ?TieContext
                a splinter:TieContext ;
                commonlib:facilityName ?FacilityName ;
                commonlib:projectCode ?ProjectCode ;
                commonlib:contractNumber ?ContractNumber .
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
                pattern = $"'{name}' ?p ?o .";
                break;
            case TripleContent.Predicate:
                pattern = $"?s '{name}' ?o .";
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