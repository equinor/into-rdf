using IntoRdf.RdfModels;
using Microsoft.Extensions.Logging;
using IntoRdf.Services.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.Services.TransformationServices.RdfTableBuilderServices;
using IntoRdf.Services.TransformationServices.RdfGraphServices;

using System.Data;
using VDS.RDF;
using IntoRdf.Public.Models;

namespace IntoRdf.Services.TransformationServices.SpreadsheetServices;

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

    public Graph ConvertToRdf(SpreadsheetTransformationDetails transformationDetails, Stream content)
    {
      
        var contentTable = GetSpreadsheetContent(content, transformationDetails);

        var processedTable = PreprocessContent(transformationDetails, contentTable);
        
        var sourceGraph = CreateGraphFromSource(processedTable);

        return sourceGraph;
    }

    private DataTable GetSpreadsheetContent(Stream content, SpreadsheetTransformationDetails transformationDetails)
    {
        var identityTargetPath = transformationDetails.TargetPathSegments.Where(x => x.IsIdentity == true);
        var identityColumn = identityTargetPath.Count() == 1 ? identityTargetPath.First().Target : null;
        return _excelDomReaderService.GetSpreadsheetData(content, transformationDetails.SpreadsheetDetails, identityColumn);
    }

    private DataTable PreprocessContent(SpreadsheetTransformationDetails transformationDetails, DataTable content)
    {
        var dataContentUri = CreateDataContentUri(transformationDetails.IriSegments);
        return _excelTableBuilderService.GetInputDataTable(dataContentUri, transformationDetails, content);
    }

    private Graph CreateGraphFromSource(DataTable content)
    {
        _rdfGraphService.AssertDataTable(content);
       return _rdfGraphService.GetGraph(); 
    }

    private Uri CreateDataContentUri(List<string> iriSegments)
    {
        var dataContentPath = Public.Utils.PrefixToUri["equinor"].AbsoluteUri;

        if (iriSegments.Count > 0)
        {
            iriSegments.ForEach(s => dataContentPath += $"{s}/");
        }
        return new Uri(dataContentPath);
    }
}