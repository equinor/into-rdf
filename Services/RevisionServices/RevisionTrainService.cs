using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.FusekiServices;
using Services.ValidationServices.RevisionTrainValidationServices;
using Services.Utils;
using Common.Exceptions;
using Common.GraphModels;
using Common.Constants;
using Common.RdfModels;
using Common.RevisionTrainModels;
using Common.Utils;
using VDS.RDF;

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

    public async Task<HttpResponseMessage> AddRevisionTrain(HttpRequest request)
    {
        string revisionTrain;
        using (var stream = new StreamReader(request.Body))
        {
            revisionTrain = await stream.ReadToEndAsync();
        }

        await ValidateTrain(revisionTrain);

        var response = await _fusekiService.AddData(_server, new ResultGraph(GraphConstants.Default, revisionTrain), "text/turtle");
        _logger.LogInformation("Successfully uploaded revision train");

        return response;
    }

    public async Task<HttpResponseMessage> RestoreRevisionTrain(string revisionTrain)
    {
        await ValidateTrain(revisionTrain);

        var response = await _fusekiService.AddData(_server, new ResultGraph(GraphConstants.Default, revisionTrain), "text/turtle");
        _logger.LogInformation("Successfully uploaded revision train");

        return response;
    }

    private async Task ValidateTrain(string revisionTrain)
    {
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
    }

    public async Task<HttpResponseMessage> GetRevisionTrainByName(string name)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            _logger.LogWarning($"Failed to get train with name {name} because it doesn't exist");
            throw new FileNotFoundException($"Failed to get train with name {name} because it doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByNameQuery(name);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var graph = new Graph();
        graph.LoadFromString(await response.Content.ReadAsStringAsync());

        var trainNameNode = graph.CreateLiteralNode(name);
        if (trainNameNode == null) { throw new FileNotFoundException($"Failed to get train with name {name}"); }

        var trainTriple = graph.GetTriples(trainNameNode).First();
        var trainUri = (UriNode) trainTriple.Subject;

        return await GetRevisionTrainByUri(trainUri.Uri);
    }

    public async Task<HttpResponseMessage> GetRevisionTrainByRecord(Uri record)
    {
        var recordExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, record.AbsoluteUri));

        if (!recordExist)
        {
            _logger.LogWarning($"Failed to get revision train with record {record} because the record doesn't exist");
            throw new FileNotFoundException($"Failed to get revision train with record {record} because the record doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByRecordQuery(record);
        var response = await _fusekiService.Query(_server, revisionTrainQuery);

        var graph = new Graph();
        graph.LoadFromString(await response.Content.ReadAsStringAsync());

        var recordUriNode = graph.CreateUriNode(record);
        var trainTriples = graph.GetTriples(recordUriNode);
        if (trainTriples.Count() != 1) { throw new FileNotFoundException($"Failed to get record {record}"); }

        var trainUri = (UriNode) trainTriples.First().Subject;

        return await GetRevisionTrainByUri(trainUri.Uri);
    }

    private async Task<HttpResponseMessage> GetRevisionTrainByUri(Uri trainUri)
    {
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, trainUri.AbsoluteUri));

        if (!trainExist)
        {
            _logger.LogWarning($"Failed to get revision train with record {trainUri} because the record doesn't exist");
            throw new FileNotFoundException($"Failed to get revision train with record {trainUri} because the record doesn't exist");
        }

        var revisionTrainQuery = GetRevisionTrainByUriQuery(trainUri);
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
        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Object, name));

        if (!trainExist)
        {
            var message = $"Failed to delete train because a train with name {name} doesn't exist";
            _logger.LogWarning(message);
            throw new ObjectNotFoundException(message);
        }

        var response = await _fusekiService.Update(_server, GetDeleteRevisionTrainQuery(name));

        return response;
    }

    public ResultGraph CreateRecordContext(RevisionTrainModel train, string revisionName, DateTime revisionDate)
    {
        var recordContext = new Graph();
        var latestRevision = train.Records.MaxBy(ng => ng.RevisionNumber);

        var revisionTrainNode = recordContext.CreateUriNode(train.TrainUri);
        var hasRecordNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/splinter#hasRecord"));

        var graphPath = $"https://rdf.equinor.com/graph/{train.TripleStore}/{train.Name}";
        var graphUri = new Uri($"{graphPath}/{revisionName}");
        var graphNode = recordContext.CreateUriNode(graphUri);

        recordContext.Assert(new Triple(revisionTrainNode, hasRecordNode, graphNode));

        var RecordClassNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/record#Record"));
        var typeOf = recordContext.CreateUriNode(RdfCommonProperties.CreateType());

        recordContext.Assert(new Triple(graphNode, typeOf, RecordClassNode));

        var hasRevisionNameNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionName"));
        var revisionNameNode = recordContext.CreateLiteralNode(revisionName); //, XmlSpecsHelper.XmlSchemaDataTypeString);

        recordContext.Assert(new Triple(graphNode, hasRevisionNameNode, revisionNameNode));

        var hasRevisionDateNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionDate"));
        var revisionDateNode = recordContext.CreateLiteralNode(DateFormatter.FormateToString(revisionDate)); //, XmlSpecsHelper.XmlSchemaDataTypeDateTime);

        recordContext.Assert(new Triple(graphNode, hasRevisionDateNode, revisionDateNode));

        var hasRevisionNumberNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionNumber"));
        var revisionNumberNode = recordContext.CreateLiteralNode($"{latestRevision?.RevisionNumber + 1 ?? 1}"); //, XmlSpecsHelper.XmlSchemaDataTypeInteger);

        recordContext.Assert(new Triple(graphNode, hasRevisionNumberNode, revisionNumberNode));

        if (latestRevision != null)
        {
            var replacesNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/record#replaces"));
            var replacesUriNode = recordContext.CreateUriNode(new Uri($"{graphPath}/{latestRevision.RevisionName}"));

            recordContext.Assert(new Triple(graphNode, replacesNode, replacesUriNode));
        }

        return new ResultGraph(graphUri.AbsoluteUri, GraphSupportFunctions.WriteGraphToString(recordContext), true);
    }

    private async Task<bool> RevisionTrainExist(string revisionTrain)
    {
        var trainGraph = new Graph();
        trainGraph.LoadFromString(revisionTrain);
        var trainIri = trainGraph.GetTriplesWithObject(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#RevisionTrain"))).Single();
        var trainName = trainGraph.GetTriplesWithPredicate(trainGraph.CreateUriNode(new Uri("https://rdf.equinor.com/splinter#name"))).Single();

        var trainExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Subject, trainIri.Subject.ToString()));
        var trainNameExist = await _fusekiQueryService.Ask(_server, GraphSupportFunctions.GetAskQuery(TripleContent.Object, trainName.Object.ToString()));

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



    private string GetRevisionTrainByRecordQuery(Uri record)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train splinter:hasRecord <{record}> .  
        }}
        WHERE
        {{
            ?train splinter:hasRecord <{record}> . 
        }}
        ";

        return query;
    }

    private string GetRevisionTrainByNameQuery(string name)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            ?train splinter:name '{name}' .
        }}
        WHERE
        {{
            ?train splinter:name '{name}' .
        }}
        ";

        return query;
    }

    private string GetRevisionTrainByUriQuery(Uri uri)
    {
        var query =
        @$"
        prefix splinter: <https://rdf.equinor.com/splinter#>

        CONSTRUCT 
        {{
            <{uri}> ?trainProp ?trainValue .
            ?trainValue ?contextProp ?contextValue . 
        }}
        WHERE
        {{
            <{uri}> a splinter:RevisionTrain ;
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

    private string GetDeleteRevisionTrainQuery(string name)
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
                (splinter:hasTieContext | splinter:hasSpreadsheetContext | splinter:hasRecord) ?context .

            ?train ?trainProperty ?obj1 .
            ?context ?contextProperty ?obj2 .
        }}
        ";

        return query;
    }
}