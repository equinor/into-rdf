using System;
using System.IO;
using System.Linq;
using VDS.RDF.Parsing;
using Xunit;
using Services.TransformationServices.XMLTransformationServices.Converters;
using VDS.RDF.Query.Expressions;
using System.Collections.Generic;
using VDS.RDF.Query;
using VDS.RDF;
using System.Text;
using VDS.RDF.Writing;

namespace Services.Tests
{
    public class AmlConverterTests
    {
        FileStream fs = new FileStream("TestData/test.aml", FileMode.Open);
        [Fact]
        public void AsserTriplesFromAML()
        {
            var amlConverter = new AmlToRdfConverter(new Uri("https://rdf.equinor.com/aml/"));
            // Given
            var graph = amlConverter.Convert(fs);
            // When
            var ts = new VDS.RDF.TripleStore();
            ts.Add(graph);
            // Then
            Assert.True(ts.Graphs.Count == 1);
            Assert.True(ts.Triples.Count() > 0);
            Assert.True(ts.Triples.All(t => t.Graph == ts.Graphs.First()));
            graph.SaveToFile("turtle.ttl", new CompressingTurtleWriter());
        }
    }
}
