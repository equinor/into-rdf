using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace Services.Tests
{
    internal class RdfTestUtils
    {
        public static void AssertTripleAsserted(Graph graph, Uri rdfSubject, Uri rdfPredicate, object rdfObject)
        {
            var ok = Ask(graph, rdfSubject, rdfPredicate, rdfObject);
            if (ok)
            {
                return;
            }
            // Rest of method is for debug purposes

            // query to list which part of triples we are missing
            var subjectOk = Ask(graph, rdfSubject, null, null);
            var predicateOk = Ask(graph, null, rdfPredicate, null);
            var objectOk = Ask(graph, null, null, rdfObject);

            var errorMsg = subjectOk && predicateOk && objectOk ?
                $"All parts of triple: {rdfSubject} {rdfPredicate} {rdfObject} exists, but this combination does not\n" :
                $"\nSubject {rdfSubject}: {(subjectOk ? "✔" : "✗")}\n" +
                $"Predicate {rdfPredicate}: {(predicateOk ? "✔" : "✗")}\n" +
                $"Object {rdfObject}: {(objectOk ? "✔" : "✗")}\n";

            //query again to get suggestions for remaining parts if at least 1 part of the tripple matched
            var debugQuerySubject = subjectOk ? rdfSubject : null;
            var debugQueryPredicate = predicateOk ? rdfPredicate : null;
            var debugQueryObject = objectOk ? rdfObject : null;

            if (subjectOk || predicateOk || objectOk)
            {
                errorMsg += "Suggestions: \n" + string.Join("\n", Select(graph, debugQuerySubject, debugQueryPredicate, debugQueryObject, 10));
            }

            Assert.True(ok, errorMsg);
        }

        private static bool Ask(Graph graph, Uri rdfSubject, Uri rdfPredicate, object rdfObject)
        {
            var tripplePattern = new TriplePattern(
                CreatePattern(graph, rdfSubject),
                CreatePattern(graph, rdfPredicate),
                CreatePattern(graph, rdfObject)
            );

            var query = QueryBuilder
                .Ask()
                .Where(tripplePattern)
                .BuildQuery();

            return ((SparqlResultSet)graph.ExecuteQuery(query)).Result;
        }

        private static List<string> Select(Graph graph, Uri rdfSubject, Uri rdfPredicate, object rdfObject, int limit)
        {

            var (subjectSelect, subjectPattern) = CreateSelectAndPattern(graph, rdfSubject, "subject");
            var (predicateSelect, predicatePattern) = CreateSelectAndPattern(graph, rdfPredicate, "predicate");
            var (objectSelect, objectPattern) = CreateSelectAndPattern(graph, rdfObject, "object");

            var tripplePattern = new TriplePattern(subjectPattern, predicatePattern, objectPattern);

            var query = QueryBuilder
                .Select(subjectSelect, predicateSelect, objectSelect)
                .Where(tripplePattern)
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

        private static List<string> FillInNewValues(List<object> existingValues, List<string> newValues)
        {
            var mergedValues = new List<string>();
            var j = 0;


            for (int i=0; i<existingValues.Count; i++) {
                if (existingValues[i] == null)
                {
                    mergedValues.Add(newValues[j]);
                    j++;
                } else
                {
                    mergedValues.Add(existingValues[i].ToString());
                }
            }
            return mergedValues;
        }

        private static (string, PatternItem) CreateSelectAndPattern(Graph graph, object uriOrLiteral, string triplePossition)
        {
            return uriOrLiteral == null ?
                ($"?{triplePossition}", CreateVariablePattern(triplePossition)) :
                ($"<{uriOrLiteral}>", CreatePattern(graph, uriOrLiteral));
        }

        private static int counter = 1;
        private static PatternItem CreatePattern(Graph graph, object value)
        {
            return value switch
            {
                string udefinedLiteral => CreateLiteralPattern(graph, udefinedLiteral),
                Uri uri => CreateUriPattern(graph, uri),
                null => CreateVariablePattern("var" + counter++),
                _ => throw new Exception()
            };
        }

        public static VariablePattern CreateVariablePattern(string variableName)
        {
            return new VariablePattern(variableName);
        }

        private static NodeMatchPattern CreateUriPattern(Graph graph, Uri uri)
        {
            return new NodeMatchPattern(graph.CreateUriNode(uri));
        }

        private static NodeMatchPattern CreateLiteralPattern(Graph graph, string literal)
        {
            return new NodeMatchPattern(graph.CreateLiteralNode(literal));
        }
    }
}
