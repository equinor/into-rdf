using IntoRdf.Public.Models;
using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;
using Xunit;

namespace IntoRdf.Tests
{
    public class DomMelReaderTests
    {
        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";
            var rdfTestUtils = new RdfTestUtils();
            var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);

            var spreadsheetDetails = CreateSpreadsheetDetails();
            var transformationDetails = CreateTransformationDetails();

            var turtle = new TransformerService().TransformSpreadsheet(spreadsheetDetails, transformationDetails, stream, RdfFormat.Turtle);
            var graph = new Graph();
            graph.LoadFromString(turtle);

            Assert.NotNull(graph);

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

        private SpreadsheetDetails CreateSpreadsheetDetails()
        {
            return new SpreadsheetDetails("sheetName", 1, 2, 1);
        }

        private TransformationDetails CreateTransformationDetails()
        {
            var baseUri = new Uri("https://rdf.equinor.com/");
            var sourceBaseUri = new Uri("https://rdf.equinor.com/source/mel#");
            var targetPathSegments = new List<TargetPathSegment>(){new TargetPathSegment("Tag Number", "test", true)};

            return new TransformationDetails(baseUri, sourceBaseUri, targetPathSegments);
        }
    }
}
