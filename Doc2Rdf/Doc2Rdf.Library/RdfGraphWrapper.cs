using System;
using System.Data;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Doc2Rdf.Library
{
    internal class RdfGraphWrapper
    {
        private Graph _graph;

        public RdfGraphWrapper()
        {
            _graph = new Graph();

            foreach (var pair in Prefixes.Prefix2Uri) {
                _graph.NamespaceMap.AddNamespace(pair.Key, pair.Value);
            }
        }

        public void AssertDataTable(DataTable dataTable)
        {

            foreach (DataRow row in dataTable.Rows)
            {
                var rdfSubject = CreateUriNode((Uri) row["id"]);

                foreach (DataColumn header in dataTable.Columns)
                {
                    if (header.ColumnName == "id" || IsNull(row[header]))
                    {
                        continue;
                    }

                    var rdfPredicate = CreateUriNode(new Uri(header.ColumnName));
                    var rdfObject = CreateNode(row[header]);

                    _graph.Assert(new Triple(rdfSubject, rdfPredicate, rdfObject));
                }
            }
        }

        public string WriteGraphToString()
        {
            MemoryStream outputStream = new MemoryStream();
            _graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
            return Encoding.UTF8.GetString(outputStream.ToArray());
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

        private ILiteralNode CreateUndefinedLiteralNode(string udefinedLiteral)
        {
            return _graph.CreateLiteralNode(udefinedLiteral);
        }

        private IUriNode CreateUriNode(Uri uri)
        {
            return _graph.CreateUriNode(Prefixes.FullForm2PrefixForm(uri));
        }
    }
}