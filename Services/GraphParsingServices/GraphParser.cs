using Common.RevisionTrainModels;
using Common.TransformationModels;
using Common.Exceptions;
using Services.Utils;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;


namespace Services.GraphParserServices;
public class GraphParser : IGraphParser
{
    public RevisionTrainModel ParseRevisionTrain(string revisionTrain)
    {
        Graph trainGraph = new Graph();
        trainGraph.LoadFromString(revisionTrain, new TurtleParser());

        var revisionTrainModel = ParseMainTrain(trainGraph);
        revisionTrainModel.TieContext = ParseTieContext(trainGraph);
        revisionTrainModel.SpreadsheetDetails = ParseSpreadsheetDetails(trainGraph);
        revisionTrainModel.Records = ParseNamedGraphs(trainGraph);

        return revisionTrainModel;
    }

    private RevisionTrainModel ParseMainTrain(Graph trainGraph)
    {
        var trainUri = GetSubjectValueForTripleWithObject(trainGraph, new Uri("https://rdf.equinor.com/splinter#RevisionTrain"));
        if (trainUri == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Train identity uri is missing"); }

        var name = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter#name"));
        if (name == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Name is missing"); }

        var tripleStore = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter#tripleStore"));
        if (tripleStore == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Triple store is missing"); }

        var trainType = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter#trainType"));
        if (trainType == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Train type is missing"); }

        return new RevisionTrainModel(trainUri, name.ToString(), tripleStore.ToString(), trainType.ToString());
    }

    private TieContext? ParseTieContext(Graph trainGraph)
    {
        var tieContextUri = GetSubjectValueForTripleWithObject(trainGraph, new Uri("https://rdf.equinor.com/splinter#TieContext"));
        if (tieContextUri == null) { return null; }

        var facilityName = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#facilityName"));
        if (facilityName == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Facility name is missing from Tie context"); }

        var objectName = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#objectName"));
        if (objectName == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Object name is missing from Tie context"); }

        var contractNumber = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#contractNumber"));
        if (contractNumber == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Contract number is missing from Tie context"); }

        var tieContext = new TieContext(facilityName.ToString(), objectName.ToString(), contractNumber.ToString());

        var projectCode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#projectCode"));
        if (projectCode != null) { tieContext.ProjectCode = projectCode.ToString(); }

        var contractorCode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#contractorCode"));
        if (contractorCode != null) { tieContext.ContractorCode = contractorCode.ToString(); }

        var documentTitle = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/commonlib/tie#documentTitle"));
        if (documentTitle != null) { tieContext.DocumentTitle = documentTitle.ToString(); }

        return tieContext;
    }

    private SpreadsheetDetails? ParseSpreadsheetDetails(Graph trainGraph)
    {
        var tieContextUri = GetSubjectValueForTripleWithObject(trainGraph, new Uri("https://rdf.equinor.com/splinter#SpreadsheetContext"));
        if (tieContextUri == null) { return null; }

        var sheetName = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#sheetName"));
        if (sheetName == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Sheet name is missing from Spreadsheet context"); }

        var headerRowNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#headerRow"));
        if (headerRowNode == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Header row is missing from Spreadsheet context"); }
        var headerRow = (int)headerRowNode.AsValuedNode().AsInteger();

        var dataStartRowNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#dataStartRow"));
        if (dataStartRowNode == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Data start row is missing from Spreadsheet context"); }
        var dataStartRow = (int)dataStartRowNode.AsValuedNode().AsInteger();

        var startColumnNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#startColumn"));
        if (startColumnNode == null) { throw new RevisionTrainValidationException("Failed to parse revision train. Start column is missing from Spreadsheet context"); }
        var startColumn = (int)startColumnNode.AsValuedNode().AsInteger();

        var spreadsheetDetails = new SpreadsheetDetails(sheetName.ToString(), headerRow, dataStartRow, startColumn);

        var dataEndRowNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#dataEndRow"));
        if (dataEndRowNode != null) { spreadsheetDetails.DataEndRow = (int)dataEndRowNode.AsValuedNode().AsInteger(); }

        var endColumnNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#endColumn"));
        if (endColumnNode != null) { spreadsheetDetails.EndColumn = (int)endColumnNode.AsValuedNode().AsInteger(); }

        var isTransposedNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#startColumn"));
        if (isTransposedNode != null) { spreadsheetDetails.IsTransposed = isTransposedNode.AsValuedNode().AsBoolean(); }

        var identityColumnNode = GetObjectNodeFromTripleWithPredicate(trainGraph, new Uri("https://rdf.equinor.com/splinter/spreadsheet#identityColumn"));
        if (identityColumnNode != null) { spreadsheetDetails.IdentityColumn = identityColumnNode.ToString(); }

        return spreadsheetDetails;
    }

    private List<RecordModel> ParseNamedGraphs(Graph trainGraph)
    {
        List<RecordModel> namedGraphs = new List<RecordModel>();

        var recordNode = trainGraph.GetUriNode(new Uri("https://rdf.equinor.com/ontology/record#Record"));
        if (recordNode == null) { return namedGraphs; }

        var namedGraphTriples = trainGraph.GetTriplesWithObject(recordNode);

        foreach (var ngt in namedGraphTriples)
        {
            var revisionNameNode = GetObjectNodeFromTripleWithSubjectAndPredicate(trainGraph, ngt.Subject, new Uri("https://rdf.equinor.com/ontology/revision#hasRevisionName"));
            if (revisionNameNode == null) { continue; }
            var revisionName = revisionNameNode.ToString();

            var revisionDateNode = GetObjectNodeFromTripleWithSubjectAndPredicate(trainGraph, ngt.Subject, new Uri("https://rdf.equinor.com/ontology/revision#hasRevisionDate"));
            if (revisionDateNode == null) { continue; }
            var revisionDate = DateFormatter.FormateToDate(revisionDateNode.ToString());

            var namedGraph = new RecordModel(((UriNode)ngt.Subject).Uri, revisionName, revisionDate);

            var revisionNumberNode = GetObjectNodeFromTripleWithSubjectAndPredicate(trainGraph, ngt.Subject, new Uri("https://rdf.equinor.com/ontology/revision#hasRevisionNumber"));
            if (revisionNumberNode != null) { namedGraph.RevisionNumber = Int32.Parse(revisionNumberNode.ToString()); }

            var replacesUriNode = GetObjectNodeFromTripleWithSubjectAndPredicate(trainGraph, ngt.Subject, new Uri("https://rdf.equinor.com/ontology/record#replaces"));
            if (replacesUriNode != null) { namedGraph.Replaces = ((UriNode)replacesUriNode).Uri; }

            namedGraphs.Add(namedGraph);
        }

        return namedGraphs;
    }

    private INode? GetObjectNodeFromTripleWithSubjectAndPredicate(Graph graph, INode subject, Uri predicateUri)
    {
        var predicateNode = graph.GetUriNode(predicateUri);
        if (predicateNode == null) { return null; }
        
        var triples = graph.GetTriplesWithSubjectPredicate(subject, predicateNode);

        if (triples.Count() != 1)
        {
            return null;
        }

        return triples.First().Object;
    }

    private INode? GetObjectNodeFromTripleWithPredicate(Graph graph, Uri uri)
    {
        var predicateNode = graph.GetUriNode(uri);
        if (predicateNode == null) { return null; }

        var triples = graph.GetTriplesWithPredicate(predicateNode);

        if (triples.Count() != 1)
        {
            return null;
        }

        return triples.First().Object;
    }

    private Uri? GetSubjectValueForTripleWithObject(Graph graph, Uri uri)
    {
        var objectNode = graph.GetUriNode(uri);
        if (objectNode == null) { return null; }

        var triples = graph.GetTriplesWithObject(objectNode);

        if (triples.Count() != 1)
        {
            return null;
        }

        return new Uri(triples.First().Subject.ToString());
    }
}