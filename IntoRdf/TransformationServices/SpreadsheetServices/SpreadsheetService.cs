using IntoRdf.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.TransformationServices.RdfTableBuilderServices;
using IntoRdf.TransformationServices.RdfGraphServices;

using System.Data;
using VDS.RDF;
using IntoRdf.Public.Models;

namespace IntoRdf.TransformationServices.SpreadsheetServices;

internal class SpreadsheetService : ISpreadsheetService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IExcelRdfTableBuilderService _excelTableBuilderService;
    private readonly IRdfGraphService _rdfGraphService;

    public SpreadsheetService(
        IExcelDomReaderService excelDomReaderService, 
        IExcelRdfTableBuilderService excelTableBuilderService,
        IRdfGraphService rdfGraphService)
    {
        _excelDomReaderService = excelDomReaderService;
        _excelTableBuilderService = excelTableBuilderService;
        _rdfGraphService = rdfGraphService;
    }

    public Graph ConvertToRdf(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content)
    {
        var contentTable = GetSpreadsheetContent(content, spreadsheetDetails);
        var processedTable = PreprocessContent(transformationDetails, contentTable);
        return CreateGraphFromSource(processedTable);
    }

    private DataTable GetSpreadsheetContent(Stream content, SpreadsheetDetails spreadsheetDetails)
    {
        return _excelDomReaderService.GetSpreadsheetData(content, spreadsheetDetails);
    }

    private DataTable PreprocessContent(TransformationDetails transformationDetails, DataTable content)
    {
        return _excelTableBuilderService.GetInputDataTable(transformationDetails, content);
    }

    private Graph CreateGraphFromSource(DataTable content)
    {
        _rdfGraphService.AssertDataTable(content);
       return _rdfGraphService.GetGraph(); 
    }
}