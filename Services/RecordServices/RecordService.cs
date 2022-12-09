using Common.Exceptions;
using Common.GraphModels;
using Common.RevisionTrainModels;
using Common.Utils;
using Repositories.OntologyRepository;
using Repositories.RecordRepository;
using Repositories.RevisionTrainRepository;
using Services.FusekiServices;
using Services.GraphParserServices;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.RevisionServices;
using Services.Utils;

namespace Services.RecordServices;

public class RecordService : IRecordService
{
    private readonly IRevisionTrainService _revisionTrainService;
    private readonly IRevisionService _revisionService;
    private readonly IEnumerable<ISpreadsheetTransformationService> _spreadsheetTransformationService;
    private readonly IFusekiService _fusekiService;
    private readonly IGraphParser _graphParser;
    private readonly IRecordRepository _recordRepository;
    private readonly IRevisionTrainRepository _revisionTrainRepository;
    private readonly IOntologyRepository _ontologyRepository;
    public RecordService(IRevisionTrainService revisionTrainService,
        IRevisionService revisionService,
        IEnumerable<ISpreadsheetTransformationService> spreadsheetTransformationService,
        IFusekiService fusekiService,
        IGraphParser graphParser,
        IRecordRepository recordRepository,
        IRevisionTrainRepository revisionTrainRepository,
        IOntologyRepository ontologyRepository)
    {
        _revisionTrainService = revisionTrainService;
        _revisionService = revisionService;
        _spreadsheetTransformationService = spreadsheetTransformationService;
        _fusekiService = fusekiService;
        _graphParser = graphParser;
        _recordRepository = recordRepository;
        _revisionTrainRepository = revisionTrainRepository;
        _ontologyRepository = ontologyRepository;
    }

    public string TransformExcel(RevisionTrainModel revisionTrain, Stream content)
    {
        var transformationService = GetTransformationService(revisionTrain.TrainType);

        return transformationService.Transform(revisionTrain, content);
    }

    public async Task<string> Add(RecordInputModel recordInput)
    {
        var trainResponse = await _revisionTrainService.GetRevisionTrainByName(recordInput.RevisionTrainName);
        var revisionTrain = await trainResponse.Content.ReadAsStringAsync();
        var revisionTrainModel = _graphParser.ParseRevisionTrain(revisionTrain);
        DateTime date = DateFormatter.FormateToDate(recordInput.RevisionDate);

        _revisionService.ValidateRevision(revisionTrainModel, recordInput.RevisionName, date);

        var transformed = String.Empty;

        switch (recordInput.ContentType)
        {
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                transformed = TransformExcel(revisionTrainModel, recordInput.Content);
                break;
            case "application/AML":
                throw new NotImplementedException("Splinter will soon have AML support");
            case "text/turtle":
                throw new NotImplementedException("WHAT? Isn't Splinter handling RDF yet? Ehhh, no");
            default:
                throw new UnsupportedContentTypeException(@$"Unsupported Media Type for IFormFile {recordInput.ContentType}.
                        Supported content types:
                            AML: application/AML,
                            Excel: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
                            RDF: text/turtle
                            ");
        }

        var ontologyGraph = await _ontologyRepository.Get(ServerKeys.Main, revisionTrainModel.TrainType);
        //TODO - Next step SourceToOntologyConversion

        var recordContext = _revisionTrainService.CreateRecordContext(revisionTrainModel, recordInput.RevisionName, date);

        await _revisionTrainRepository.AddRecordContext(recordContext);
        try
        {
            await _recordRepository.Add(revisionTrainModel.TripleStore, new ResultGraph(recordContext.Name, transformed));
        }
        catch
        {
            await _revisionTrainRepository.DeleteRecordContext(new Uri(recordContext.Name));
            throw;
        }

        return recordContext.Name;
    }

    public async Task<HttpResponseMessage> Delete(string server, Uri record)
    {
        return await _fusekiService.Update(server, GetDropRecordQuery(record.AbsoluteUri));
    }

    public async Task<HttpResponseMessage> Delete(string server, List<Uri> records)
    {
        string deleteAllQuery = string.Empty;
        foreach (var r in records)
        {
            deleteAllQuery += GetDropRecordQuery(r.AbsoluteUri);
        }

        return await _fusekiService.Update(server, deleteAllQuery);
    }

    private ISpreadsheetTransformationService GetTransformationService(string trainType)
    {
        return _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == trainType) ??
            throw new RevisionTrainValidationException($"A transformer of for train type {trainType} is not available.");
    }

    private string GetDropRecordQuery(string record)
    {
        return $"DROP GRAPH <{record}> ;";
    }
}