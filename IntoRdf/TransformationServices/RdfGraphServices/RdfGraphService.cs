using System.Data;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace IntoRdf.TransformationServices.RdfGraphServices;

internal class RdfGraphService : IRdfGraphService
{
    private Graph _graph;

    public RdfGraphService()
    {
        _graph = InitializeGraph();
    }

    private Graph InitializeGraph()
    {
        return new Graph();
    }

    public void AssertDataTable(DataTable dataTable)
    {
        _graph.Merge(AssertRawData(dataTable));
    }

    public void AssertDataTable(DataTable dataTable, string subjectColumn)
    {
        _graph.Merge(AssertRawData(dataTable, subjectColumn));
    }

    public Graph GetGraph()
    {
        return _graph;
    }

    private Graph AssertRawData(DataTable dataTable)
    {
        return AssertRawData(dataTable, "id");
    }

    private Graph AssertRawData(DataTable dataTable, string subjectColumn)
    {
        Graph graph = InitializeGraph();
        foreach (DataRow row in dataTable.Rows)
        {
            var test = row[subjectColumn];
            var rdfSubject = CreateUriNode((Uri)row[subjectColumn]);

            foreach (DataColumn header in dataTable.Columns)
            {
                if (header.ColumnName == subjectColumn || IsNull(row[header]))
                {
                    continue;
                }

                var rdfPredicate = CreateUriNode(new Uri(header.ColumnName));
                var rdfObject = CreateNode(row[header]);

                foreach (INode node in rdfObject)
                {
                    graph.Assert(new Triple(rdfSubject.First(), rdfPredicate.First(), node));
                }
            }
        }
        return graph;
    }

    private bool IsNull(object value)
    {
        return value == null || value == DBNull.Value || value.ToString() == string.Empty;
    }

    private IList<INode> CreateNode(object value)
    {
        return value switch
        {
            string stringLiteral => CreateStringLiteralNode(stringLiteral),
            int intLiteral => CreateIntLiteral(intLiteral),
            Int64 intLiteral => CreateLongLiteral(intLiteral),
            Double doubleLiteral => CreateDoubleLiteral(doubleLiteral),
            Uri uri => CreateUriNode(uri),
            DateTime dateTime => CreateDateTimeLiteral(dateTime),
            Array arrayLiteral => CreateLiteralsTypedByArray(arrayLiteral),
            Boolean booleanLiteral => CreateBooleanLiteral(booleanLiteral),
            _ => HandleError(value)
        };
    }


    private static IList<INode> HandleError(object value)
    {
        throw new Exception($"Unknown datatype {value.GetType()}");
    }
    private IList<INode> CreateBooleanLiteral(bool booleanLiteral)
    {
        return new List<INode>() { _graph.CreateLiteralNode(booleanLiteral.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)) };
    }
    private IList<INode> CreateLiteralsTypedByArray(Array arrayLiteral)
    {
        List<INode> returnList = new List<INode>();
        foreach (object obj in arrayLiteral)
        {
            if (obj is not null)
            {
                returnList.AddRange(CreateNode(obj));
            }
        }
        return returnList;
    }

    private IList<INode> CreateDateTimeLiteral(DateTime literal)
    {
        return new List<INode>() { _graph.CreateLiteralNode(literal.ToUniversalTime().ToString("o"), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) };
    }

    private IList<INode> CreateIntLiteral(int literal)
    {
        return new List<INode>() { _graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInt)) };
    }
    private IList<INode> CreateLongLiteral(Int64 literal)
    {
        return new List<INode>() { _graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeLong)) };
    }
    private IList<INode> CreateDoubleLiteral(Double literal)
    {
        return new List<INode>() { _graph.CreateLiteralNode(literal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)) };
    }

    private IList<INode> CreateStringLiteralNode(string literal)
    {
        return new List<INode>() { _graph.CreateLiteralNode(literal, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)) };
    }

    private IList<INode> CreateUriNode(Uri uri)
    {
        return new List<INode>() { _graph.CreateUriNode(uri) };
    }
}