using IntoRdf.RdfModels;
using System.Data;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Services.TransformationServices.RdfGraphServices;

public class RdfGraphService : IRdfGraphService
{
    private Graph _graph;

    public RdfGraphService()
    {
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

    public void AssertDataTable(DataTable dataTable)
    {
        _graph.Merge(AssertRawData(dataTable));
    }

    public Graph GetGraph()
    {
        return _graph;
    }

    private Graph AssertRawData(DataTable dataTable)
    {
        Graph graph = InitializeGraph();
        foreach (DataRow row in dataTable.Rows)
        {
            var test = row["id"];
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

    private bool IsNull(object value)
    {
        return value == null || value == DBNull.Value || value.ToString() == string.Empty;
    }

    private INode CreateNode(object value)
    {
        return value switch
        {
            string stringLiteral => CreateStringLiteralNode(stringLiteral),
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

    private ILiteralNode CreateDateTimeLiteral(DateTime literal)
    {
        return _graph.CreateLiteralNode(literal.ToUniversalTime().ToString("o"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
    }

    private ILiteralNode CreateIntLiteral(int literal)
    {
        return _graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInt));
    }

    private ILiteralNode CreateStringLiteralNode(string literal)
    {
        return _graph.CreateLiteralNode(literal, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
    }

    private IUriNode CreateUriNode(Uri uri)
    {
        return _graph.CreateUriNode(uri);
    }
}