using System.Data;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace IntoRdf.TransformationServices;

internal class RdfAssertionService : IRdfAssertionService
{
    public Graph AssertProcessedData(DataTable dataTable)
    {
        return AssertProcessedData(dataTable, DataTableProcessor.SubjectColumnName);
    }

    public Graph AssertProcessedData(DataTable dataTable, string subjectColumnName)
    {
        Graph graph = new Graph();

        //Add version of IntoRdf to the graph
        var versionSubject = CreateUriNode(graph, new Uri("https://example.com/into-rdf"));
        var versionPredicate = CreateUriNode(graph, new Uri("https://example.com/hasVersion"));
        var versionObject = CreateStringLiteralNode(graph, GetIntoRdfVersion());
        graph.Assert(new Triple(versionSubject.First(), versionPredicate.First(), versionObject.First()));

        foreach (DataRow row in dataTable.Rows)
        {
            var subject = GetObjectString(row[subjectColumnName]);
            if (subject == null) continue;

            var rdfSubject = CreateUriNode(graph, new Uri(subject));

            foreach (DataColumn header in dataTable.Columns)
            {
                if (header.ColumnName == subjectColumnName || IsNull(row[header]))
                {
                    continue;
                }

                var rdfPredicate = CreateUriNode(graph, new Uri(header.ColumnName));
                var rdfObject = CreateNode(graph, row[header]);

                foreach (INode node in rdfObject)
                {
                    graph.Assert(new Triple(rdfSubject.First(), rdfPredicate.First(), node));
                }
            }
        }
        return graph;
    }

    private static string? GetObjectString(object cell)
    {
        if (IsNull(cell)) { return null; }

        return cell.ToString();
    }

    private static bool IsNull(object value)
    {
        return value == null || value == DBNull.Value || value.ToString() == string.Empty;
    }

    private static IList<INode> CreateNode(Graph graph, object value)
    {
        return value switch
        {
            string stringLiteral => CreateStringLiteralNode(graph, stringLiteral),
            int intLiteral => CreateIntLiteral(graph, intLiteral),
            Int64 intLiteral => CreateLongLiteral(graph, intLiteral),
            Double doubleLiteral => CreateDoubleLiteral(graph, doubleLiteral),
            Uri uri => CreateUriNode(graph, uri),
            DateTime dateTime => CreateDateTimeLiteral(graph, dateTime),
            Array arrayLiteral => CreateLiteralsTypedByArray(graph, arrayLiteral),
            Boolean booleanLiteral => CreateBooleanLiteral(graph, booleanLiteral),
            _ => HandleError(value)
        };
    }

    private static IList<INode> HandleError(object value)
    {
        throw new Exception($"Unknown datatype {value.GetType()}");
    }

    private static IList<INode> CreateBooleanLiteral(Graph graph, bool booleanLiteral)
    {
        return new List<INode>() { graph.CreateLiteralNode(booleanLiteral.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)) };
    }

    private static IList<INode> CreateLiteralsTypedByArray(Graph graph, Array arrayLiteral)
    {
        List<INode> returnList = new List<INode>();
        foreach (object obj in arrayLiteral)
        {
            if (obj is not null)
            {
                returnList.AddRange(CreateNode(graph, obj));
            }
        }
        return returnList;
    }

    private static IList<INode> CreateDateTimeLiteral(Graph graph, DateTime literal)
    {
        return new List<INode>() { graph.CreateLiteralNode(literal.ToUniversalTime().ToString("o"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) };
    }

    private static IList<INode> CreateIntLiteral(Graph graph, int literal)
    {
        return new List<INode>() { graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInt)) };
    }

    private static IList<INode> CreateLongLiteral(Graph graph, Int64 literal)
    {
        return new List<INode>() { graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)) };
    }

    private static IList<INode> CreateDoubleLiteral(Graph graph, Double literal)
    {
        return new List<INode>() { graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)) };
    }

    private static IList<INode> CreateStringLiteralNode(Graph graph, string literal)
    {
        return new List<INode>() { graph.CreateLiteralNode(literal, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)) };
    }

    private static IList<INode> CreateUriNode(Graph graph, Uri uri)
    {
        return new List<INode>() { graph.CreateUriNode(uri) };
    }

    private static string GetIntoRdfVersion()
    {
        var doc = new XmlDocument();
        doc.Load("IntoRdf.csproj");

        var versionNode = doc.SelectSingleNode("Version");
        if (versionNode != null)
        {
            return versionNode.InnerText;
        }

        // Ha noe feilhåndtering her
        return null;
    }
}