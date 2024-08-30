using IntoRdf.DomReaderServices.ExcelDomReaderServices;

using System.Data;
using VDS.RDF;
using IntoRdf.Models;

namespace IntoRdf.TransformationServices.SpreadsheetServices;

internal class SpreadsheetService : ISpreadsheetService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IDataTableProcessor _dataTableProcessor;
    private readonly IRdfAssertionService _rdfAssertionService;

    public SpreadsheetService(
        IExcelDomReaderService excelDomReaderService,
        IDataTableProcessor excelTableBuilderService,
        IRdfAssertionService rdfAssertionService)
    {
        _excelDomReaderService = excelDomReaderService;
        _dataTableProcessor = excelTableBuilderService;
        _rdfAssertionService = rdfAssertionService;
    }

    public Graph ConvertToRdf(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content)
    {
        var contentTable = GetSpreadsheetContent(content, spreadsheetDetails);
        var processedTable = _dataTableProcessor.ProcessDataTable(transformationDetails, contentTable);
        return CreateGraphFromSource(processedTable);
    }

    private DataTable GetSpreadsheetContent(Stream content, SpreadsheetDetails spreadsheetDetails)
    {
        return _excelDomReaderService.GetSpreadsheetData(content, spreadsheetDetails);
    }

    private Graph CreateGraphFromSource(DataTable content)
    {
        return _rdfAssertionService.AssertProcessedData(content);
    }
}