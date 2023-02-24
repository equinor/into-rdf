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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Services.Tests
{
    public class AmlConverterTests
    {
        FileStream fs = new FileStream("TestData/NotSoTest.aml", FileMode.Open);
        [Fact]
        public void AsserTriplesFromAML()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<AmlToRdfConverter>();
            var amlConverter = new AmlToRdfConverter(
                new Uri("https://rdf.equinor.com/jsv/scd/"),
                logger,
                new List<Uri>(),
                new List<(string,Uri)>()
                    { ("IAmString" ,new Uri("https://iAmAlsoExample.com"))
                    }
                );
            var graph = amlConverter.Convert(fs);
            
            var ts = new VDS.RDF.TripleStore();
            ts.Add(graph);
            
            Assert.True(ts.Graphs.Count == 1);
            Assert.True(ts.Triples.Count() > 0);
            Assert.True(ts.Triples.All(t => t.Graph == ts.Graphs.First()));
            if (File.Exists("turtle.ttl")) File.Delete("turtle.ttl");
            graph.SaveToFile("turtle.ttl", new CompressingTurtleWriter());
            if (File.Exists("store.trig")) File.Delete("store.trig");
            ts.SaveToFile("store.trig", new TriGWriter());
        }
    }
}