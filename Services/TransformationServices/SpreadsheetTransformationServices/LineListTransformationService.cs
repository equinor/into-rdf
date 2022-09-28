using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTransformationServices;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

 public class LineListTransformationService: ISpreadsheetTransformationService
 {
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IRdfTransformationService _rdfTransformerService;
    private ILogger<LineListTransformationService> _logger;
    private string _dataSource;

    public LineListTransformationService(IExcelDomReaderService excelDomReaderService, IRdfTransformationService rdfTransformerService, ILogger<LineListTransformationService> logger)
    {
        _excelDomReaderService = excelDomReaderService;
        _rdfTransformerService = rdfTransformerService;
        _logger = logger;
        _dataSource = DataSource.LineList;
    }

    public ResultGraph Transform(Provenance provenance, Graph ontology, BlobDownloadResult blob, SpreadsheetDetails details)
    {
        using Stream excelStream = blob.Content.ToStream();
        
        _logger.LogInformation("<LineListTransformationService> - Transform: Start parsing of spreadsheet data");

        var data = _excelDomReaderService.GetSpreadsheetData(excelStream, details);

        _logger.LogInformation("<LineListTransformationService> - Transform: Spreadsheet table with {numberOfColumns} columns and {numberOfRows} rows retrieved", data.Columns.Count, data.Rows.Count);

        return _rdfTransformerService.Transform(provenance, ontology, data);
    }

    public ResultGraph Transform(Stream excelStream, Graph ontology, string fileName)
    {
        throw new NotImplementedException();
    }

    public ResultGraph Transform(Stream excelStream, Graph ontology, SpreadsheetInfo details)
    {
        throw new NotImplementedException();
    }

    public string GetDataSource()
    {
        return _dataSource;
    } 
 }