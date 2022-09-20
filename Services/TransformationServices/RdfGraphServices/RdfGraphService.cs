using Common.RdfModels;
using Services.TransformationServices.SourceToOntologyConversionService;
using System.Data;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Services.TransformationServices.RdfGraphServices;

public class RdfGraphService : IRdfGraphService
{
    private Graph _graph;
    private readonly ISourceToOntologyConversionService _sourceVocabularyConversionService;

    public RdfGraphService(ISourceToOntologyConversionService sourceVocabularyConversionService)
    {
        _sourceVocabularyConversionService = sourceVocabularyConversionService;
        _graph = InitializeGraph();
    }

    private Graph InitializeGraph()
    {
        var graph = new Graph();

        foreach (var pair in RdfPrefixes.Prefix2Uri)
        {
            graph.NamespaceMap.AddNamespace(pair.Key, pair.Value);
        }

        return graph;
    }

    public void AssertDataTable(DataTable dataTable, Graph ontologyGraph)
    {
        _graph.Merge(AssertRawData(dataTable));
        if (dataTable.TableName == "InputData")
        {
            _graph.Merge(AssertOntologyConvertedData(dataTable, ontologyGraph));
        }
    }

    public string WriteGraphToString()
    {
        using MemoryStream outputStream = new MemoryStream();
        
        _graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    private Graph AssertRawData(DataTable dataTable)
    {
        Graph graph = InitializeGraph();
        foreach (DataRow row in dataTable.Rows)
        {
            var rdfSubject = CreateUriNode((Uri)row["id"]);

            foreach (DataColumn header in dataTable.Columns)
            {
                if (header.ColumnName == "id" || IsNull(row[header]))
                {
                    continue;
                }

                var rdfPredicate = CreateUriNode(new Uri(header.ColumnName));
                var rdfObject = CreateNode(row[header]);

                graph.Assert(new Triple(rdfSubject, rdfPredicate, rdfObject));
            }
        }
        return graph;
    }

    private Graph AssertOntologyConvertedData(DataTable dataTable, Graph ontologyGraph)
    {        
        Graph graph = new Graph();
        if (!ontologyGraph.IsEmpty)
        {
            _sourceVocabularyConversionService.ConvertSourceToOntology(dataTable, ontologyGraph);
            graph = _sourceVocabularyConversionService.GetGraph();
        }

        return graph;
    }

    private bool IsNull(object value)
    {
        return value == null || value == DBNull.Value || value.ToString() == string.Empty;
    }

    private INode CreateNode(object value)
    {
        return value switch
        {
            string undefinedLiteral => CreateUndefinedLiteralNode(undefinedLiteral),
            int intLiteral => CreateIntLiteral(intLiteral),
            Uri uri => CreateUriNode(uri),
            DateTime dateTime => CreateDateTimeLiteral(dateTime),
            _ => HandleError(value)
        };
    }

    private static INode HandleError(object value)
    {
        throw new Exception($"Unknown datatype {value.GetType()}");
    }

    private ILiteralNode CreateDateTimeLiteral(DateTime dateTime)
    {
        return _graph.CreateLiteralNode(dateTime.ToUniversalTime().ToString("o"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
    }

    private ILiteralNode CreateIntLiteral(int intLiteral)
    {
        return _graph.CreateLiteralNode(intLiteral.ToString(), new Uri(RdfPrefixes.Prefix2Uri["xsd"] + "int"));
    }

    private ILiteralNode CreateUndefinedLiteralNode(string undefinedLiteral)
    {
        return _graph.CreateLiteralNode(undefinedLiteral);
    }

    private IUriNode CreateUriNode(Uri uri)
    {
        return _graph.CreateUriNode(uri);
    }
}