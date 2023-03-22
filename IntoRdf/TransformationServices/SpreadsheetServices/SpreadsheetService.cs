using IntoRdf.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.TransformationServices.RdfGraphServices;

using System.Data;
using VDS.RDF;
using IntoRdf.Public.Models;

namespace IntoRdf.TransformationServices.SpreadsheetServices;

internal class SpreadsheetService : ISpreadsheetService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IDataTableProcessor _dataTableProcessor;
    private readonly IRdfGraphService _rdfGraphService;

    public SpreadsheetService(
        IExcelDomReaderService excelDomReaderService, 
        IDataTableProcessor excelTableBuilderService,
        IRdfGraphService rdfGraphService)
    {
        _excelDomReaderService = excelDomReaderService;
        _dataTableProcessor = excelTableBuilderService;
        _rdfGraphService = rdfGraphService;
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
        _rdfGraphService.AssertDataTable(content);
       return _rdfGraphService.GetGraph(); 
    }
}