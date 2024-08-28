using System.Data;
using System.Reflection;
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
        IRefNode activity = graph.CreateBlankNode();

        AddProvenance(graph, activity);

        foreach (DataRow row in dataTable.Rows)
        {
            var subject = GetObjectString(row[subjectColumnName]);
            if (subject == null) continue;

            var rdfSubject = CreateUriNode(graph, new Uri(subject));

            var wasGeneratedByPredicate = new Triple(
                rdfSubject.First(),
                new UriNode(new Uri(Namespaces.Prov.WasGeneratedBy)),
                activity);

            graph.Assert(wasGeneratedByPredicate);

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

    private void AddProvenance(Graph graph, IRefNode activity)
    {
        var versionUri = new UriNode(new Uri(CreateIntoRdfVersionUri()));
        graph.Assert(new Triple(
            activity,
            new UriNode(new Uri(Namespaces.Prov.WasAssociatedWith)),
            versionUri));

        graph.Assert(new Triple(
            versionUri,
            new UriNode(new Uri(Namespaces.Rdfs.Comment)),
            new LiteralNode("Version of IntoRdf used to translate this data.")));
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

    private string CreateIntoRdfVersionUri()
    {
        var outputFolderPath = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream("IntoRdf.Properties.commit.url") ??
                               throw new Exception("Could not get IntoRdf commit url.");
        var shapeString = new StreamReader(outputFolderPath).ReadToEnd();
        return shapeString;
    }
}