using IntoRdf.TransformationModels;
using Services.TransformationServices.SpreadsheetServices;
using Services.TransformationServices.OntologyServices;
using Services.TransformationServices.RecordService;
using VDS.RDF;
using VDS.RDF.Parsing;
using IntoRdf.Utils;

namespace Services.TransformerServices;

public class TransformerService : ITransformerService
{
    private readonly ISpreadsheetService _spreadsheetService;
    private readonly IOntologyService _ontologyService;
    private readonly IRecordTransformationService _recordService;

    public TransformerService(ISpreadsheetService spreadsheetService, 
        IOntologyService ontologyService,
        IRecordTransformationService recordService)
    {
        _spreadsheetService = spreadsheetService;
        _ontologyService = ontologyService;
        _recordService = recordService;
    }

    public string TransformSpreadsheet(SpreadsheetTransformationDetails transformationDetails, Stream content)
    {
        var graph = _spreadsheetService.ConvertToRdf(transformationDetails, content);
        return GraphSupportFunctions.WriteGraphToString(graph, RdfWriterType.Turtle);
    }

    public string EnrichRdf(string ontologyString, string graphString)
    {
        var source = new Graph();
        source.LoadFromString(graphString, new TurtleParser());

        var ontology = new Graph();
        ontology.LoadFromString(ontologyString);

        var enriched = _ontologyService.EnrichRdf(ontology, source);  
        return GraphSupportFunctions.WriteGraphToString(enriched, RdfWriterType.Turtle);    
    }

    public string CreateProtoRecord(Uri record, string graphString )
    {
        var graph = new Graph();
        graph.LoadFromString(graphString);
        var protoRecord = _recordService.CreateProtoRecord(record, graph);
        return GraphSupportFunctions.WriteGraphToString(protoRecord, RdfWriterType.Jsonld);   
    }
}