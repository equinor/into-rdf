using Doc2Rdf.Library;
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
        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";
            string data = null;

            using (var file = File.Open(testFile, FileMode.Open, FileAccess.Read))
            {
                data = Doc2RdfTransformer.Transform(file);
            }

            Assert.NotNull(data);

            var graph = new Graph();
            var parser = new TurtleParser();
            parser.Load(graph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(data))));

            //Provenance
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01"),
                new Uri("http://rdf.equinor.com/ext/project#projectCode"),
                "c232"
            );
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01"),
                new Uri("http://rdf.equinor.com/ontology/sor#file"),
                "test.xlsx"
            );
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=2"),
                new Uri("http://www.w3.org/ns/prov#wasDerivedFrom"),
                new Uri("http://rdf.equinor.com/ext/mel/c232_01")
            );

            //Actual Data
            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=2"),
                new Uri("http://rdf.equinor.com/raw/melexcel#Header3"),
                "1729"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=2"),
                new Uri("http://rdf.equinor.com/raw/melexcel#Header4"),
                "3300.375"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=500"),
                new Uri("http://rdf.equinor.com/raw/melexcel#Header55"),
                "BC500"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=499"),
                new Uri("http://rdf.equinor.com/raw/melexcel#Header55"),
                "BC499"
            );

            RdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("http://rdf.equinor.com/ext/mel/c232_01#row=498"),
                new Uri("http://rdf.equinor.com/raw/melexcel#Header53"),
                "BA498"
            );
        }
    }
}
