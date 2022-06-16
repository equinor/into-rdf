using Doc2Rdf.Library;
using Doc2Rdf.Library.Interfaces;
using System;
using System.IO;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace Doc2Rdf.Tests
{
    public class DomMelReaderTests
    {
        private readonly IMelTransformer _melTransformer;

        public DomMelReaderTests(IMelTransformer melTransformer)
        {
            _melTransformer = melTransformer;
        }

        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";

            var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);
            var data = _melTransformer.Transform(stream, testFile);

            Assert.NotNull(data);

            var graph = new Graph();
            var parser = new TurtleParser();
            parser.Load(graph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(data))));

            //Provenance
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01"),
                new Uri("https://rdf.equinor.com/ontology/facility#hasDocumentProjectId"),
                new Uri("https://rdf.equinor.com/ontology/facility#C232")
            );
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01"),
                new Uri("https://rdf.equinor.com/ontology/sor#fromDataCollection"),
                "test.xlsx"
            );
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01"),
                new Uri("http://www.w3.org/ns/prov#hadMember"),
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=2")          
            );

            //Actual Data
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=2"),
                new Uri("https://rdf.equinor.com/source/mel#Header3"),
                "1729"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=2"),
                new Uri("https://rdf.equinor.com/source/mel#Header4"),
                "3300.375"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=499"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC500"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=498"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC499"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/c232/mel/01#row=497"),
                new Uri("https://rdf.equinor.com/source/mel#Header53"),
                "BA498"
            );
        }
    }
}
