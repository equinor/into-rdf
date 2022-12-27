using Common.RevisionTrainModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTransformationServices;
using VDS.RDF;  

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public class SpreadsheetTransformationService : ISpreadsheetTransformationService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IRdfTransformationService _rdfTransformerService;
    private readonly ILogger<SpreadsheetTransformationService> _log;

    public SpreadsheetTransformationService(
        IExcelDomReaderService excelDomReaderService, 
        IRdfTransformationService rdfTransformerService, 
        ILogger<SpreadsheetTransformationService> log)
    {
        _excelDomReaderService = excelDomReaderService;
        _rdfTransformerService = rdfTransformerService;
        _log = log;
    }

    public Graph Transform(RevisionTrainModel revisionTrain, Stream content)
    {
        if(revisionTrain.SpreadsheetContext == null) { throw new InvalidOperationException("Missing spreadsheet context"); }
        _log.LogInformation("<SpreadsheetTransformer> - Transform: Starting parsing of spreadsheet data");
        
        var data = _excelDomReaderService.GetSpreadsheetData(content, revisionTrain.SpreadsheetContext);
        _log.LogInformation("<SpreadsheetTransformer> - Transform: Spreadsheet table with {numberOfColumns} columns and {numberOfRows} rows retrieved", data.Columns.Count, data.Rows.Count);

        return _rdfTransformerService.Transform(revisionTrain, data);
    }
}