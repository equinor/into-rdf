using IntoRdf.Public.Models;
using System;
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

            var transformationDetails = CreateTransformationDetails();

            var turtle = new TransformerService().TransformSpreadsheet(transformationDetails, stream, RdfFormat.Turtle);
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

        private SpreadsheetTransformationDetails CreateTransformationDetails()
        {
            var spreadsheetDetails = new SpreadsheetDetails("sheetName", 1, 2, 1);

            var transformationDetails = new SpreadsheetTransformationDetails(new Uri("https://rdf.equinor.com/test"), spreadsheetDetails);
            transformationDetails.TransformationType = "mel";
            transformationDetails.Level = EnrichmentLevel.None;

            var identityColumn = new TargetPathSegment("Tag Number", "test", true);
            transformationDetails.TargetPathSegments.Add(identityColumn);

            return transformationDetails;
        }
    }
}
