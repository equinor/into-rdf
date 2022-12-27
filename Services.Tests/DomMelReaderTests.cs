using Common.ProvenanceModels;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.GraphParserServices;
using Common.Utils;
using System;
using System.IO;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace Services.Tests
{
    public class DomMelReaderTests
    {
        private readonly ISpreadsheetTransformationService _spreadsheetTransformationService;
        private readonly IGraphParser _graphParser;

        public DomMelReaderTests(ISpreadsheetTransformationService transformationServices, IGraphParser graphParser)
        {
            _spreadsheetTransformationService = transformationServices;
            _graphParser = graphParser;
        }

        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";
            var testTrainFile = "TestData/testTrain.ttl";
            var rdfTestUtils = new RdfTestUtils(DataSource.Mel);
            var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);

            using FileStream fs = new FileStream(testTrainFile, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            string trainContent = sr.ReadToEnd();

            var trainRevisionModel = _graphParser.ParseRevisionTrain(trainContent);

            var resultGraph = _spreadsheetTransformationService.Transform(trainRevisionModel, stream);
            var result = GraphSupportFunctions.WriteGraphToString(resultGraph);

            Assert.NotNull(resultGraph);

            var graph = new Graph();
            var parser = new TurtleParser();
            parser.Load(graph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(result))));

            //Actual Data
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A1"),
                new Uri("https://rdf.equinor.com/source/mel#Header3"),
                "1729"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A1"),
                new Uri("https://rdf.equinor.com/source/mel#Header4"),
                "3300.375"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A499"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC500"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A498"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC499"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A497"),
                new Uri("https://rdf.equinor.com/source/mel#Header53"),
                "BA498"
            );
        }


    }
}
