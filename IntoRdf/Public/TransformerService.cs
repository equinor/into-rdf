using IntoRdf.TransformationServices.SpreadsheetServices;
using IntoRdf.Utils;
using IntoRdf.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using IntoRdf.Models;
using IntoRdf.TransformationServices.XMLTransformationServices.Converters;
using IntoRdf.Validation;
using IntoRdf.TransformationServices;

namespace IntoRdf;

public class TransformerService : ITransformerService
{
    private readonly ICsvService _csvService;
    private readonly ISpreadsheetService _spreadsheetService;
    private readonly ITabularJsonTransformationService _tabularJsonTransformationService;

    public TransformerService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var provider = collection.BuildServiceProvider();

        _csvService = provider.GetService<ICsvService>() ?? throw new Exception("Unable to resolve ICsvService");
        _spreadsheetService = provider.GetService<ISpreadsheetService>() ?? throw new Exception("Unable to resolve ISpreadsheetService");
        _tabularJsonTransformationService = provider.GetService<ITabularJsonTransformationService>() ?? throw new Exception("Unable to resolve ITabularJsonTransofrmationService");
    }

    public string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails)
    {
        TransformationDetailsValidation.ValidateTransformationDetails(transformationDetails);
        return _tabularJsonTransformationService.TransformTabularJson(content, outputFormat, subjectProperty, transformationDetails);
    }

    public string TransformSpreadsheet(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content)
    {
        TransformationDetailsValidation.ValidateTransformationDetails(transformationDetails);
        var graph = _spreadsheetService.ConvertToRdf(spreadsheetDetails, transformationDetails, content);
        return GraphSupportFunctions.WriteGraphToString(graph, transformationDetails.OutputFormat);
    }

    public string TransformCsv(CsvDetails csvDetails, TransformationDetails transformationDetails, Stream content)
    {
        TransformationDetailsValidation.ValidateTransformationDetails(transformationDetails);
        var graph = _csvService.ConvertToRdf(csvDetails, transformationDetails, content);
        return GraphSupportFunctions.WriteGraphToString(graph, transformationDetails.OutputFormat);
    }

    public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat)
    {
        var amlTransformer = new AmlToRdfConverter(transformationDetails.BaseUri, transformationDetails.IdentityCollectionsAndPatternsArgs);
        var graph = amlTransformer.Convert(content);
        return GraphSupportFunctions.WriteGraphToString(graph, outputFormat);
    }
}