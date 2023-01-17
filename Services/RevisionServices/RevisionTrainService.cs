using Microsoft.Extensions.Logging;
using Repositories.RecordRepository;
using Repositories.RevisionTrainRepository;
using Services.FusekiServices;
using Services.GraphParserServices;
using Services.ValidationServices.RevisionTrainValidationServices;
using Services.Utils;
using Common.Exceptions;
using Common.GraphModels;
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
    private readonly IGraphParser _graphParser;
    private readonly IRecordRepository _recordRepository;
    private readonly IRevisionTrainRepository _revisionTrainRepository;
    private readonly ILogger<RevisionTrainService> _logger;
    private readonly string _server;

    public RevisionTrainService(
        IFusekiQueryService fusekiQueryService,
        IFusekiService fusekiService,
        IRevisionTrainValidator revisionTrainValidator,
        IGraphParser graphParser,
        IRecordRepository recordRepository,
        IRevisionTrainRepository revisionTrainRepository,
        ILogger<RevisionTrainService> logger)
    {
        _fusekiQueryService = fusekiQueryService;
        _fusekiService = fusekiService;
        _revisionTrainValidator = revisionTrainValidator;
        _graphParser = graphParser;
        _recordRepository = recordRepository;
        _revisionTrainRepository = revisionTrainRepository;
        _logger = logger;
        _server = ServerKeys.Main;
    }

    public async Task<string> Add(string revisionTrain)
    {
        await ValidateTrain(revisionTrain);
        await _revisionTrainRepository.Add(revisionTrain);

        return GetRevisionTrainId(revisionTrain);
    }

    public async Task<string> GetByName(string name)
    {
        return await _revisionTrainRepository.GetByName(name);
    }

    public async Task<string> GetByRecord(Uri record)
    {
        return await _revisionTrainRepository.GetByRecord(record);
    }

    public async Task<string> GetAll()
    {
        return await _revisionTrainRepository.GetAll();
    }

    public async Task Delete(string name)
    {
        var revisionTrain = await GetByName(name);
        var revisionTrainModel = _graphParser.ParseRevisionTrain(revisionTrain);

        var records = revisionTrainModel.Records.Select(r => r.RecordUri).ToList();

        await _revisionTrainRepository.Delete(name);

        try
        {
            await _recordRepository.Delete(revisionTrainModel.TripleStore, records);
        }
        catch
        {
            await ValidateTrain(revisionTrain);
            await _revisionTrainRepository.Add(revisionTrain);
            throw;
        }
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

        return new ResultGraph(graphUri.AbsoluteUri, GraphSupportFunctions.WriteGraphToString(recordContext, RdfWriterType.Turtle), true);
    }

    private async Task ValidateTrain(string revisionTrain)
    {
        if (revisionTrain == string.Empty)
        {
            _logger.LogWarning("Failed to create revision train because the train is empty");
            throw new ObjectNotFoundException("Failed to create revision train because the train is empty");
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

    private async Task<bool> RevisionTrainExist(string revisionTrain)
    {
        var trainGraph = GraphSupportFunctions.LoadGraphFromString(revisionTrain);
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

    private string GetRevisionTrainId(string revisionTrain)
    {
        var trainGraph = GraphSupportFunctions.LoadGraphFromString(revisionTrain);
        var triples = trainGraph.GetTriplesWithObject(trainGraph.GetUriNode(new Uri("https://rdf.equinor.com/splinter#RevisionTrain")));

        if (triples.Count() != 1)
        {
            throw new InvalidOperationException($"Revision train contains wrong number of individuals: {triples.Count()}. Expected 1.");
        }

        return triples.First().Subject.ToString();
    }
}