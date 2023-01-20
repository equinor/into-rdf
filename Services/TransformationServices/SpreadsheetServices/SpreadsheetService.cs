using Common.RdfModels;
using Common.TransformationModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTableBuilderServices;
using Services.TransformationServices.RdfGraphServices;

using System.Data;
using VDS.RDF;  

namespace Services.TransformationServices.SpreadsheetServices;

public class SpreadsheetService : ISpreadsheetService
{
    private readonly IExcelDomReaderService _excelDomReaderService;
    private readonly IExcelRdfTableBuilderService _excelTableBuilderService;
    private readonly IRdfGraphService _rdfGraphService;
    private readonly ILogger<SpreadsheetService> _log;

    public SpreadsheetService(
        IExcelDomReaderService excelDomReaderService, 
        IExcelRdfTableBuilderService excelTableBuilderService,
        IRdfGraphService rdfGraphService,
        ILogger<SpreadsheetService> log)
    {
        _excelDomReaderService = excelDomReaderService;
        _excelTableBuilderService = excelTableBuilderService;
        _rdfGraphService = rdfGraphService;
        _log = log;
    }

    public Graph ConvertToRdf(SpreadsheetTransformationDetails transformationDetails, Stream content)
    {
        _log.LogInformation("<SpreadsheetTransformer> - Transform: Starting parsing of spreadsheet data");
        
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
        var dataContentPath = RdfPrefixes.Prefix2Uri["equinor"].AbsoluteUri;

        if (iriSegments.Count > 0)
        {
            iriSegments.ForEach(s => dataContentPath += $"{s}/");
        }
        return new Uri(dataContentPath);
    }
}