using Common.Exceptions;
using Common.Constants;
using Common.GraphModels;
using Common.RdfModels;
using Common.RevisionTrainModels;
using Common.Utils;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.RevisionServices;
using VDS.RDF;
using System.Text;
using VDS.RDF.Writing;

namespace Services.RecordServices;

public class RecordService : IRecordService
{
    private readonly IRevisionTrainService _revisionTrainService;
    private readonly IOntologyService _ontologyService;
    private readonly IEnumerable<ISpreadsheetTransformationService> _spreadsheetTransformationService;
    private readonly IFusekiService _fusekiService;
    public RecordService(IRevisionTrainService revisionTrainService,
        IOntologyService ontologyService, 
        IEnumerable<ISpreadsheetTransformationService> spreadsheetTransformationService,
        IFusekiService fusekiService)
    {
        _revisionTrainService = revisionTrainService;
        _ontologyService = ontologyService;
        _spreadsheetTransformationService = spreadsheetTransformationService;
        _fusekiService = fusekiService;
    }

    public async Task<HttpResponseMessage> UploadExcel(RevisionTrainModel revisionTrain, ResultGraph recordContextResult, Stream content)
    {
        //var ontology = await _ontologyService.GetSourceOntologies(revisionTrain.TrainType);

        var transformationService = GetTransformationService(revisionTrain.TrainType);
        var record = transformationService.Transform(revisionTrain, content);

        var recordContextResponse = await _fusekiService.AddData(ServerKeys.Main, recordContextResult, "text/turtle");

        var recordResultGraph = new ResultGraph(recordContextResult.Name, record, false);
        var recordResponse = await _fusekiService.AddData(revisionTrain.TripleStore, recordResultGraph, "text/turtle");

        return recordResponse;
    }

    public ResultGraph CreateRecordContext(RevisionTrainModel train, string revisionName, DateTime revisionDate)
    {
        var recordContext = new Graph();
        var latestRevision = train.NamedGraphs.MaxBy(ng => ng.RevisionNumber);

        var revisionTrainNode = recordContext.CreateUriNode(train.TrainUri);
        var hasRecordNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/splinter#hasRecord"));

        var graphPath = $"https://rdf.equinor.com/graph/{train.TripleStore}/{train.Name}";
        var graphUri = new Uri($"{graphPath}/{revisionName}");
        var graphNode = recordContext.CreateUriNode(graphUri);

        recordContext.Assert(new Triple(revisionTrainNode, hasRecordNode, graphNode));

        var RecordClassNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/record#Record"));
        var typeOf = recordContext.CreateUriNode(RdfCommonProperties.CreateType());

        recordContext.Assert(new Triple(graphNode, typeOf, RecordClassNode));

        var hasRevisionNameNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionName"));
        var revisionNameNode = recordContext.CreateLiteralNode(revisionName); //, XmlSpecsHelper.XmlSchemaDataTypeString);

        recordContext.Assert(new Triple(graphNode, hasRevisionNameNode, revisionNameNode));

        var hasRevisionDateNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionDate"));
        var revisionDateNode = recordContext.CreateLiteralNode(revisionDate.ToString()); //, XmlSpecsHelper.XmlSchemaDataTypeDateTime);

        recordContext.Assert(new Triple(graphNode, hasRevisionDateNode, revisionDateNode));

        var hasRevisionNumberNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/revision#hasRevisionNumber"));
        var revisionNumberNode = recordContext.CreateLiteralNode($"{latestRevision?.RevisionNumber + 1 ?? 1}"); //, XmlSpecsHelper.XmlSchemaDataTypeInteger);

        recordContext.Assert(new Triple(graphNode, hasRevisionNumberNode, revisionNumberNode));

        if (latestRevision != null)
        {
            var replacesNode = recordContext.CreateUriNode(new Uri($"https://rdf.equinor.com/ontology/record#replaces"));
            var replacesUriNode = recordContext.CreateUriNode(new Uri($"{graphPath}/{latestRevision.RevisionName}"));

            recordContext.Assert(new Triple(graphNode, replacesNode, replacesUriNode));
        }

        return new ResultGraph(graphUri.AbsoluteUri, WriteGraphToString(recordContext), true);
    }

    private ISpreadsheetTransformationService GetTransformationService(string trainType)
    {
        return _spreadsheetTransformationService.FirstOrDefault(transformer => transformer.GetDataSource() == trainType) ??
            throw new RevisionTrainValidationException($"A transformer of for train type {trainType} is not available.");
    }

    private string WriteGraphToString(Graph graph)
    {
        using MemoryStream outputStream = new MemoryStream();
        
        graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }
}