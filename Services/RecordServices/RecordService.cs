using Common.Exceptions;
using Common.GraphModels;
using Common.RevisionTrainModels;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.RevisionServices;

namespace Services.RecordServices;

public class RecordService : IRecordService
{
    private readonly IRevisionTrainService _revisionTrainService;
    private readonly IOntologyService _ontologyService;
    private readonly IEnumerable<ISpreadsheetTransformationService> _spreadsheetTransformationService;
    private readonly IFusekiService _fusekiService;
    public RecordService(IRevisionTrainService revisionTrainService,
        IOntologyService ontologyService, 
        IEnumerable<ISpreadsheetTransformationService> spreadsheetTransformationService,
        IFusekiService fusekiService)
    {
        _revisionTrainService = revisionTrainService;
        _ontologyService = ontologyService;
        _spreadsheetTransformationService = spreadsheetTransformationService;
        _fusekiService = fusekiService;
    }

    public string TransformExcel(RevisionTrainModel revisionTrain, Stream content)
    {
        var transformationService = GetTransformationService(revisionTrain.TrainType);

        return transformationService.Transform(revisionTrain, content);
    }

    public async Task<HttpResponseMessage> Add(string server, ResultGraph record)
    {
        return await _fusekiService.AddData(server, record, "text/turtle");
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