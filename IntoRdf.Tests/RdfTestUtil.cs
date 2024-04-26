using IntoRdf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing;
using Xunit;

namespace IntoRdf.Tests
{
    internal class RdfTestUtil
    {
        private Graph _graph;
        private TransformationDetails _transformationDetails;
        private const string xsdPrefix = "http://www.w3.org/2001/XMLSchema#";

        public RdfTestUtil(string testFile, SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails)
        {
            _transformationDetails = transformationDetails;

            using var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);
            var turtle = new TransformerService().TransformSpreadsheet(spreadsheetDetails, transformationDetails, stream);
            _graph = new Graph();
            _graph.LoadFromString(turtle);
        }

        internal string WriteGraphToString(RdfFormat writerType)
        {
            using MemoryStream outputStream = new MemoryStream();
            switch (writerType)
            {
                case RdfFormat.Trig:
                    _graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new TriGWriter());
                    break;
                case RdfFormat.Jsonld:
                    _graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new JsonLdWriter());
                    break;
                case RdfFormat.Turtle:
                    _graph.SaveToStream(new StreamWriter(outputStream, new UTF8Encoding(false)), new CompressingTurtleWriter());
                    break;
                default:
                    throw new InvalidOperationException($"Unknown RDF writer type {writerType}");
            }
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        internal void AssertTripleCount(int number)
        {
            Assert.Equal(number, _graph.Triples.Count());
        }

        internal void AssertTripleAsserted(string subjectSuffix, string predicateSuffix, object rdfObject)
        {
            AssertTripleAsserted(
                new Uri($"{_transformationDetails.BaseUri}{subjectSuffix}"),
                new Uri($"{_transformationDetails.SourcePredicateBaseUri}{predicateSuffix}"),
                rdfObject);
        }

        internal void AssertTripleAsserted(Uri rdfSubject, Uri rdfPredicate, object rdfObject)
        {
            var ok = Ask(_graph, rdfSubject, rdfPredicate, rdfObject);
            if (ok)
            {
                return;
            }
            // Rest of method is for debug purposes

            // query to list which part of triples we are missing
            var subjectOk = Ask(_graph, rdfSubject, null, null);
            var predicateOk = Ask(_graph, null, rdfPredicate, null);
            var objectOk = Ask(_graph, null, null, rdfObject);

            var errorMsg = subjectOk && predicateOk && objectOk ?
                $"All parts of triple: {rdfSubject} {rdfPredicate} {rdfObject} exists, but this combination does not\n" :
                $"\nSubject {rdfSubject}: {(subjectOk ? "✔" : "✗")}\n" +
                $"Predicate {rdfPredicate}: {(predicateOk ? "✔" : "✗")}\n" +
                $"Object {rdfObject}: {(objectOk ? "✔" : "✗")}\n";

            //query again to get suggestions for remaining parts if at least 1 part of the triple matched
            var debugQuerySubject = subjectOk ? rdfSubject : null;
            var debugQueryPredicate = predicateOk ? rdfPredicate : null;
            var debugQueryObject = objectOk ? rdfObject : null;

            if (subjectOk || predicateOk || objectOk)
            {
                errorMsg += "Suggestions: \n" + string.Join("\n", Select(_graph, debugQuerySubject, debugQueryPredicate, debugQueryObject, 10));
            }

            Assert.True(ok, errorMsg);
        }

        internal void AssertObjectExist(Dictionary<string, object> expectedProps)
        {
            var subjects = GetSubjects(expectedProps);
            var debugMsg = $"Expected to find a single object, but found {subjects.Count} objects satisfing the predicate-object pairs: [\n{DebugProps(expectedProps, 2)}\n]";
            if (subjects.Count != 1)
            {
                Assert.Fail(debugMsg);
            }

            var actualProps = GetAllProperties(subjects.Single());
            var debugMsgToBigObject = $"Expected to find a single object satisfing only the props: [\n{DebugProps(expectedProps, 2)}\n], but found an object containing additional props. Found props: [\n{DebugProps(actualProps, 2)}\n]";
            if (actualProps.Count > expectedProps.Count)
            {
                Assert.Fail(debugMsgToBigObject);
            }
        }

        internal List<string> GetAllSubjects()
        {
            const string subject = "subject";
            var (selectSubject, whereSubject) = CreateSelectAndPattern(_graph, null, subject);
            var (_selectPredicate, wherePredicate) = CreateSelectAndPattern(_graph, null, "predicate");
            var (_selectObject, whereObject) = CreateSelectAndPattern(_graph, null, "object");

            var query = QueryBuilder
                .Select(selectSubject)
                .Where(new TriplePattern(whereSubject, wherePredicate, whereObject))
            .BuildQuery();

            var result = (SparqlResultSet)_graph.ExecuteQuery(query);
            return result.Select(r => r[subject].ToString()).Distinct().ToList();
        }

        private string DebugProps(Dictionary<string, object> props, int tabCount)
        {
            var tabs = new string('\t', tabCount);
            return tabs + string.Join("\n" + tabs, props.Select(pair => $"{pair.Key}: {pair.Value}"));
        }

        private List<Uri> GetSubjects(Dictionary<string, object> predicateObjectPairs)
        {
            var variable = "subject";
            var (selectVar, whereVar) = CreateSelectAndPattern(_graph, null, "subject");
            var patterns = predicateObjectPairs.Select(pair => new TriplePattern(
                whereVar,
                CreatePattern(_graph, new Uri(pair.Key)),
                CreatePattern(_graph, pair.Value)
            ));

            var selectQuery = QueryBuilder
                .Select(selectVar)
                .Where(patterns.ToArray())
            .BuildQuery();

            var subjectResult = (SparqlResultSet)_graph.ExecuteQuery(selectQuery);
            return subjectResult.Select(r => new Uri(r[variable].ToString())).ToList();
        }

        private Dictionary<string, object> GetAllProperties(Uri subject)
        {
            var predicateVar = "predicate";
            var objectVar = "object";
            var (selectPredicate, wherePredicate) = CreateSelectAndPattern(_graph, null, predicateVar);
            var (selectObject, whereObject) = CreateSelectAndPattern(_graph, null, objectVar);

            var pattern = new TriplePattern(
                CreateUriPattern(_graph, subject),
                wherePredicate,
                whereObject
            );

            var propQuery = QueryBuilder
                .Select(selectPredicate, selectObject)
                .Where(pattern)
            .BuildQuery();

            var propQueryResult = (SparqlResultSet)_graph.ExecuteQuery(propQuery);
            return propQueryResult.ToDictionary(
                result => result[predicateVar].ToString(),
                result => (object)result[objectVar].ToString()
            );
        }

        private bool Ask(Graph graph, Uri rdfSubject, Uri rdfPredicate, object rdfObject)
        {
            var triplePattern = new TriplePattern(
                CreatePattern(graph, rdfSubject),
                CreatePattern(graph, rdfPredicate),
                CreatePattern(graph, rdfObject)
            );

            var query = QueryBuilder
                .Ask()
                .Where(triplePattern)
                .BuildQuery();

            return ((SparqlResultSet)graph.ExecuteQuery(query)).Result;
        }

        private List<string> Select(Graph graph, Uri rdfSubject, Uri rdfPredicate, object rdfObject, int limit)
        {
            var (subjectSelect, subjectPattern) = CreateSelectAndPattern(graph, rdfSubject, "subject");
            var (predicateSelect, predicatePattern) = CreateSelectAndPattern(graph, rdfPredicate, "predicate");
            var (objectSelect, objectPattern) = CreateSelectAndPattern(graph, rdfObject, "object");

            var triplePattern = new TriplePattern(subjectPattern, predicatePattern, objectPattern);

            var query = QueryBuilder
                .Select(subjectSelect, predicateSelect, objectSelect)
                .Where(triplePattern)
                .Limit(limit)
                .BuildQuery();

            var existingValues = new List<object> { rdfSubject, rdfPredicate, rdfObject };

            var resultSet = (SparqlResultSet)graph.ExecuteQuery(query);
            var debug = query.ToString();
            var mergedValues = resultSet.Select(
                r => string.Join(
                    " - ",
                    FillInNewValues(
                        existingValues,
                        r.Select(pair => pair.Value.ToString()).ToList()
                    )
               )
            );

            return mergedValues.ToList();
        }

        private List<string> FillInNewValues(List<object> existingValues, List<string> newValues)
        {
            var mergedValues = new List<string>();
            var j = 0;


            for (int i = 0; i < existingValues.Count; i++)
            {
                if (existingValues[i] == null)
                {
                    mergedValues.Add(newValues[j]);
                    j++;
                }
                else
                {
                    mergedValues.Add(existingValues[i].ToString());
                }
            }
            return mergedValues;
        }

        private (string, PatternItem) CreateSelectAndPattern(Graph graph, object uriOrLiteral, string triplePosition)
        {
            return uriOrLiteral == null ?
                ($"?{triplePosition}", CreateVariablePattern(triplePosition)) :
                ($"<{uriOrLiteral}>", CreatePattern(graph, uriOrLiteral));
        }


        private static int counter = 1;
        private PatternItem CreatePattern(Graph graph, object value)
        {
            return value switch
            {
                string stringLiteral => CreateStringLiteralPattern(graph, stringLiteral),
                double doubleLiteral => CreateDoubleLiteralPattern(graph, doubleLiteral),
                int intLiteral => CreateIntLiteralPattern(graph, intLiteral),
                Uri uri => CreateUriPattern(graph, uri),
                null => CreateVariablePattern("var" + counter++),
                _ => throw new Exception()
            };

        }

        private static VariablePattern CreateVariablePattern(string variableName)
        {
            return new VariablePattern(variableName);
        }

        private static NodeMatchPattern CreateUriPattern(Graph graph, Uri uri)
        {
            return new NodeMatchPattern(graph.CreateUriNode(uri));
        }

        private static NodeMatchPattern CreateStringLiteralPattern(Graph graph, string literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(xsdPrefix + "string")));
        }

        private static NodeMatchPattern CreateDoubleLiteralPattern(Graph graph, double literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(xsdPrefix + "double")));
        }

        private static NodeMatchPattern CreateIntLiteralPattern(Graph graph, int literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(xsdPrefix + "int")));
        }

        public List<Triple> GetTriples()
        {
            return _graph.Triples.ToList();
        }
    }
}
