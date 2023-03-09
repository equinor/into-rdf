using IntoRdf.TransformationServices.SpreadsheetServices;
using IntoRdf.TransformationServices.OntologyServices;
using IntoRdf.TransformationServices.RecordService;
using VDS.RDF;
using VDS.RDF.Parsing;
using IntoRdf.Utils;
using IntoRdf.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using IntoRdf.Public.Models;
using IntoRdf.TransformationServices.XMLTransformationServices.Converters;

namespace IntoRdf;

public class TransformerService : ITransformerService
{
    private readonly ISpreadsheetService _spreadsheetService;
    private readonly IOntologyService _ontologyService;
    private readonly IRecordTransformationService _recordTransformationService;
    private readonly ITabularJsonTransformationService _tabularJsonTransformationService;

    public TransformerService()
    {
        var collection = new ServiceCollection();
        collection.AddServices();
        var provider = collection.BuildServiceProvider();

        _spreadsheetService = provider.GetService<ISpreadsheetService>() ?? throw new Exception("Unable to resolve ISpreadsheetService");
        _ontologyService = provider.GetService<IOntologyService>() ?? throw new Exception("Unable to resolve IOntologyService");
        _recordTransformationService = provider.GetService<IRecordTransformationService>() ?? throw new Exception("Unable to resolve IRecordTransformationService");
        _tabularJsonTransformationService = provider.GetService<ITabularJsonTransformationService>() ?? throw new Exception("Unable to resolve ITabularJsonTransofrmationService");
    }

    public string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails) {
        return _tabularJsonTransformationService.TransformTabularJson(content, outputFormat,subjectProperty, transformationDetails);
    }

    public string TransformSpreadsheet(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content, RdfFormat outputFormat)
    {
        var graph = _spreadsheetService.ConvertToRdf(spreadsheetDetails, transformationDetails, content);
        return GraphSupportFunctions.WriteGraphToString(graph, outputFormat);
    }

    public string InferFromOntology(string ontologyString, string graphString, RdfFormat outputFormat)
    {
        var source = new Graph();
        source.LoadFromString(graphString, new TurtleParser());

        var ontology = new Graph();
        ontology.LoadFromString(ontologyString);

        var enriched = _ontologyService.EnrichRdf(ontology, source);
        return GraphSupportFunctions.WriteGraphToString(enriched, outputFormat);
    }

    public string CreateProtoRecord(Uri record, string graphString, RdfFormat outputFormat)
    {
        var graph = new Graph();
        graph.LoadFromString(graphString);
        var protoRecord = _recordTransformationService.CreateProtoRecord(record, graph);
        return GraphSupportFunctions.WriteGraphToString(protoRecord, outputFormat);
    }

    public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat)
    {
        var amlTransformer = new AmlToRdfConverter(transformationDetails.BaseUri, transformationDetails.Scopes, transformationDetails.IdentityCollectionsAndPatternsArgs);
        var graph = amlTransformer.Convert(content);
        return GraphSupportFunctions.WriteGraphToString(graph, outputFormat);
    }
}