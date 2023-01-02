using Common.Exceptions;
using Common.GraphModels;
using Common.RevisionTrainModels;
using Common.Utils;
using Repositories.OntologyRepository;
using Repositories.RecordRepository;
using Repositories.RecordContextRepository;
using Repositories.RevisionTrainRepository;
using Services.FusekiServices;
using Services.GraphParserServices;
using Services.TransformationServices.SourceToOntologyConversionService;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.RevisionServices;
using Services.Utils;
using VDS.RDF;

namespace Services.RecordServices;

public class RecordService : IRecordService
{
    private readonly IRevisionTrainService _revisionTrainService;
    private readonly IRevisionService _revisionService;
    private readonly ISpreadsheetTransformationService _spreadsheetTransformationService;
    private readonly IFusekiService _fusekiService;
    private readonly IGraphParser _graphParser;
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordContextRepository _recordContextRepository;
    private readonly IRevisionTrainRepository _revisionTrainRepository;
    private readonly IOntologyRepository _ontologyRepository;
    private readonly ISourceToOntologyConversionService _sourceConversionService;
    public RecordService(IRevisionTrainService revisionTrainService,
        IRevisionService revisionService,
        ISpreadsheetTransformationService spreadsheetTransformationService,
        IFusekiService fusekiService,
        IGraphParser graphParser,
        IRecordRepository recordRepository,
        IRecordContextRepository recordContextRepository,
        IRevisionTrainRepository revisionTrainRepository,
        IOntologyRepository ontologyRepository,
        ISourceToOntologyConversionService sourceToOntologyConversion)
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

        var converted = ontologyGraph.IsEmpty ? transformed : _sourceConversionService.ConvertSourceToOntology(transformed, ontologyGraph);
        var convertedString = GraphSupportFunctions.WriteGraphToString(converted);

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

    public async Task<string> Transform(string revisionTrainName, Stream content, string contentType)
    {
        var revisionTrain = await _revisionTrainService.GetByName(revisionTrainName);
        var revisionTrainModel = _graphParser.ParseRevisionTrain(revisionTrain);

        var transformed = TransformToRdf(revisionTrainModel, content, contentType);

        var ontologyGraph = await _ontologyRepository.Get(ServerKeys.Main, revisionTrainModel.TrainType);
        var converted = ontologyGraph.IsEmpty ? transformed : _sourceConversionService.ConvertSourceToOntology(transformed, ontologyGraph);

        return GraphSupportFunctions.WriteGraphToString(converted);
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
                transformed = TransformExcel(revisionTrainModel, content);
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

    private Graph TransformExcel(RevisionTrainModel revisionTrain, Stream content)
    {
        return _spreadsheetTransformationService.Transform(revisionTrain, content);
    }


}