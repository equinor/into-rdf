﻿using IntoRdf.Public.Models;
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
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(Public.Utils.PrefixToUri["xsd"].ToString() + "string")));
        }

        private static NodeMatchPattern CreateDoubleLiteralPattern(Graph graph, double literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(Public.Utils.PrefixToUri["xsd"].ToString() + "double")));
        }

        private static NodeMatchPattern CreateIntLiteralPattern(Graph graph, int literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal.ToString(), new Uri(Public.Utils.PrefixToUri["xsd"].ToString() + "int")));
        }
    }
}