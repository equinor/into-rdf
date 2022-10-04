using Common.GraphModels;
using Common.ProvenanceModels;
using Services.TransformationServices.SpreadsheetTransformationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace Services.Tests
{
    public class DomMelReaderTests
    {
        private readonly IEnumerable<ISpreadsheetTransformationService> _transformationServices;

        public DomMelReaderTests(IEnumerable<ISpreadsheetTransformationService> transformationServices)
        {
            _transformationServices = transformationServices;
        }

        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";
            var rdfTestUtils = new RdfTestUtils(DataSource.Mel);
            var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);

            var melTransformationService = _transformationServices.FirstOrDefault(service => service.GetDataSource() == DataSource.Mel) ?? 
                                                throw new ArgumentException($"Transformer of type {DataSource.Mel} not available");

            //Empty ontology as it is not tested here.
            var ontology = new Graph();

            var resultGraph = melTransformationService.Transform(stream, ontology, testFile);

            Assert.NotNull(resultGraph.Content);

            var graph = new Graph();
            var parser = new TurtleParser();
            parser.Load(graph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(resultGraph.Content))));

            //Provenance
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01"),
                new Uri("https://rdf.equinor.com/ontology/sor#hasDocumentName"),
                "test"
            );
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01"),
                new Uri("https://rdf.equinor.com/ontology/sor#fromDataCollection"),
                "test.xlsx"
            );
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01"),
                new Uri("http://www.w3.org/ns/prov#hadMember"),
                new Uri("https://rdf.equinor.com/kra/test/01#row=2")          
            );

            //Actual Data
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01#row=2"),
                new Uri("https://rdf.equinor.com/source/mel#Header3"),
                "1729"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01#row=2"),
                new Uri("https://rdf.equinor.com/source/mel#Header4"),
                "3300.375"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01#row=499"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC500"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01#row=498"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC499"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/kra/test/01#row=497"),
                new Uri("https://rdf.equinor.com/source/mel#Header53"),
                "BA498"
            );
        }
    }
}
