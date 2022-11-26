using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using Common.SpreadsheetModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTransformationServices;
using VDS.RDF;  

namespace Services.TransformationServices.SpreadsheetTransformationServices;

public class MelTransformationService : ISpreadsheetTransformationService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IRdfTransformationService _rdfTransformerService;
    private readonly ILogger<MelTransformationService> _logger;
    private string _dataSource;

    public MelTransformationService(IExcelDomReaderService excelDomReaderService, IRdfTransformationService rdfTransformerService, ILogger<MelTransformationService> logger)
    {
        _excelDomReaderService = excelDomReaderService;
        _rdfTransformerService = rdfTransformerService;
        _logger = logger;
        _dataSource = DataSource.Mel;
    }

    public string Transform(RevisionTrainModel revisionTrain, Stream content)
    {
        if(revisionTrain.SpreadsheetContext == null) { throw new InvalidOperationException("Missing spreadsheet context"); }
        _logger.LogInformation("<MelTransformer> - Transform: Starting parsing of spreadsheet data from TIE message");
        
        var data = _excelDomReaderService.GetSpreadsheetData(content, revisionTrain.SpreadsheetContext);
        _logger.LogInformation("<MelTransformer> - Transform: Spreadsheet table with {numberOfColumns} columns and {numberOfRows} rows retrieved", data.Columns.Count, data.Rows.Count);

        return _rdfTransformerService.Transform(revisionTrain, data);
    }

    public string GetDataSource()
    {
        return _dataSource;
    }
}