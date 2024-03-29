using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using VDS.RDF;
using IntoRdf.Models;

namespace IntoRdf.Tests
{
    public class TabularJsonTests
    {
        [Fact]
        internal void FruitsToRdf()
        {
            FileStream fruits = new FileStream("TestData/tabular.json", FileMode.Open);
            var identifierSegment = new TargetPathSegment("id", "id");
            var td = new TransformationDetails(new Uri("https://IAmTabularJson.Something/"), new Uri("https://IAmTabularJson.Something/predicate#"), identifierSegment, new List<TargetPathSegment>(), RdfFormat.Turtle);
            var fruitTransformer = new TransformerService();
            var fruitsAsRdf = fruitTransformer.TransformTabularJson(fruits, RdfFormat.Turtle, "id", td);
            var graph = new VDS.RDF.Graph();
            graph.LoadFromString(fruitsAsRdf);
            Assert.True(graph.Triples.Any());
        }
        [Fact]
        internal void FruitsWithNestingToRdf()
        {
            FileStream fruits = new FileStream("TestData/tabular-nestedArray.json", FileMode.Open);
            var identifierSegment = new TargetPathSegment("id", "id");
            var td = new TransformationDetails(new Uri("https://IAmTabularJson.Something/"), new Uri("https://IAmTabularJson.Something/predicate#"), identifierSegment, new List<TargetPathSegment>(), RdfFormat.Turtle);
            var fruitTransformer = new TransformerService();
            var fruitsAsRdf = fruitTransformer.TransformTabularJson(fruits, RdfFormat.Turtle, "id", td);
            var graph = new VDS.RDF.Graph();
            graph.LoadFromString(fruitsAsRdf);
            var InedibletriplesFromArray = graph.GetTriplesWithPredicate(graph.GetUriNode(new Uri("https://iamtabularjson.something/predicate#inedibleParts")));
            Assert.True(InedibletriplesFromArray.Count() == 4);
            var weightTriplesFromArray = graph.GetTriplesWithPredicate(graph.GetUriNode(new Uri("https://iamtabularjson.something/predicate#weights")));
            Assert.True(weightTriplesFromArray.Count() == 7);
        }
    }
}