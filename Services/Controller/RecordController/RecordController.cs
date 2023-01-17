using Common.Exceptions;
using Common.GraphModels;
using Common.RevisionTrainModels;
using Common.TransformationModels;
using Common.Utils;
using Repositories.OntologyRepository;
using Repositories.RecordRepository;
using Repositories.RecordContextRepository;
using Repositories.RevisionTrainRepository;
using Services.FusekiServices;
using Services.GraphParserServices;
using Services.TransformationServices.OntologyServices;
using Services.TransformationServices.SpreadsheetServices;
using Services.RevisionServices;
using Services.Utils;
using VDS.RDF;

namespace Controller.RecordController;

public class RecordController : IRecordController
{
    private readonly IRevisionTrainService _revisionTrainService;
    private readonly IRevisionService _revisionService;
    private readonly ISpreadsheetService _spreadsheetTransformationService;
    private readonly IFusekiService _fusekiService;
    private readonly IGraphParser _graphParser;
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordContextRepository _recordContextRepository;
    private readonly IRevisionTrainRepository _revisionTrainRepository;
    private readonly IOntologyRepository _ontologyRepository;
    private readonly IOntologyService _sourceConversionService;
    public RecordController(IRevisionTrainService revisionTrainService,
        IRevisionService revisionService,
        ISpreadsheetService spreadsheetTransformationService,
        IFusekiService fusekiService,
        IGraphParser graphParser,
        IRecordRepository recordRepository,
        IRecordContextRepository recordContextRepository,
        IRevisionTrainRepository revisionTrainRepository,
        IOntologyRepository ontologyRepository,
        IOntologyService sourceToOntologyConversion)
    {
        _revisionTrainService = revisionTrainService;
        _revisionService = revisionService;
        _spreadsheetTransformationService = spreadsheetTransformationService;
        _fusekiService = fusekiService;
        _graphParser = graphParser;
        _recordRepository = recordRepository;
        _recordContextRepository = recordContextRepository;
        _revisionTrainRepository = revisionTrainRepository;
        _ontologyRepository = ontologyRepository;
        _sourceConversionService = sourceToOntologyConversion;
    }

    public async Task<string> Add(RecordInputModel recordInput)
    {
        var revisionTrain = await _revisionTrainService.GetByName(recordInput.RevisionTrainName);
        var revisionTrainModel = _graphParser.ParseRevisionTrain(revisionTrain);
        DateTime date = DateFormatter.FormateToDate(recordInput.RevisionDate);

        _revisionService.ValidateRevision(revisionTrainModel, recordInput.RevisionName, date);

        var transformed = TransformToRdf(revisionTrainModel, recordInput.Content, recordInput.ContentType);

        var ontologyGraph = await _ontologyRepository.Get(ServerKeys.Main, revisionTrainModel.TrainType);

        var converted = ontologyGraph.IsEmpty ? transformed : _sourceConversionService.EnrichRdf(ontologyGraph, transformed);
        var convertedString = GraphSupportFunctions.WriteGraphToString(converted, RdfWriterType.Turtle);

        var recordContext = _revisionTrainService.CreateRecordContext(revisionTrainModel, recordInput.RevisionName, date);

        await _recordContextRepository.Add(recordContext);
        try
        {
            await _recordRepository.Add(revisionTrainModel.TripleStore, new ResultGraph(recordContext.Name, convertedString));
        }
        catch
        {
            await _recordContextRepository.Delete(new Uri(recordContext.Name));
            throw;
        }

        return recordContext.Name;
    }

    public async Task Delete(Uri record)
    {
        var revisionTrain = await _revisionTrainService.GetByRecord(record);
        var revisionTrainModel = _graphParser.ParseRevisionTrain(revisionTrain);

        var latestRecord = revisionTrainModel.Records.MaxBy(rec => rec.RevisionNumber);

        if (latestRecord == null) { throw new ObjectNotFoundException($"Failed to delete record {record.AbsoluteUri} because it doesn't exist."); }

        if (latestRecord.RecordUri != record) { throw new InvalidOperationException($"Failed to delete record {record.AbsoluteUri}. The latest revision {latestRecord.RecordUri.AbsoluteUri} must be deleted first"); }

        await _recordContextRepository.Delete(latestRecord.RecordUri);
        try
        {
            await _recordRepository.Delete(revisionTrainModel.TripleStore, latestRecord.RecordUri);
        }
        catch
        {
            await _recordContextRepository.Add(new ResultGraph(record.ToString(), revisionTrain, true));
            throw;
        }
    }

    private Graph TransformToRdf(RevisionTrainModel revisionTrainModel, Stream content, string contentType)
    {
        var transformed = new Graph();

        switch (contentType)
        {
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                //transformed = revisionTrainModel.SpreadsheetDetails != null 
                //    ? TransformSpreadsheet(revisionTrainModel.SpreadsheetDetails, content) 
                //    : throw new InvalidOperationException($"Missing Spreadsheet context for revision train {revisionTrainModel.Name}");
                break;
            case "application/AML":
                throw new NotImplementedException("Splinter will soon have AML support");
            case "text/turtle":
                throw new NotImplementedException("WHAT? Isn't Splinter handling RDF yet? Ehhh, no");
            default:
                throw new UnsupportedContentTypeException(@$"Unsupported Media Type for IFormFile {contentType}.
                        Supported content types:
                            AML: application/AML,
                            Excel: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
                            RDF: text/turtle
                            ");
        }

        return transformed;
    }

    private async Task<Graph> TransformSpreadsheet(SpreadsheetTransformationDetails transformationSettings, Stream content)
    {
        var ontology = await GetOntology(transformationSettings);
        return _spreadsheetTransformationService.ConvertToRdf(transformationSettings, content);
    }

    private async Task<Graph> GetOntology(SpreadsheetTransformationDetails transformationSettings)
    {
        if (transformationSettings.Level != EnrichmentLevel.None || transformationSettings.TransformationType == null)
        {
            return new Graph();
        }

        return await _ontologyRepository.Get(ServerKeys.Main, transformationSettings.TransformationType);
    }
}