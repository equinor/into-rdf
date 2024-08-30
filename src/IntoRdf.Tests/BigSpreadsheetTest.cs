using IntoRdf.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IntoRdf.Tests;

public class BigSpreadsheetTest
{
    private readonly Uri DataUri = new Uri("https://rdf.equinor.com/");
    private readonly Uri PredicateUri = new Uri("https://rdf.equinor.com/source/mel#");
    private const string SheetName = "sheetName";

    [Fact]
    public void TestDomParsing()
    {
        var rdfTestUtils = new RdfTestUtil("TestData/test.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());

        //Actual Data
        rdfTestUtils.AssertTripleAsserted(
            new Uri("https://rdf.equinor.com/test/A1"),
            new Uri("https://rdf.equinor.com/source/mel#Header3"),
            "1729"
        );

        rdfTestUtils.AssertTripleAsserted(
            new Uri("https://rdf.equinor.com/test/A1"),
            new Uri("https://rdf.equinor.com/source/mel#Header4"),
            "3300.375"
        );

        rdfTestUtils.AssertTripleAsserted(
            new Uri("https://rdf.equinor.com/test/A499"),
            new Uri("https://rdf.equinor.com/source/mel#Header55"),
            "BC500"
        );

        rdfTestUtils.AssertTripleAsserted(
            new Uri("https://rdf.equinor.com/test/A498"),
            new Uri("https://rdf.equinor.com/source/mel#Header55"),
            "BC499"
        );

        rdfTestUtils.AssertTripleAsserted(
            new Uri("https://rdf.equinor.com/test/A497"),
            new Uri("https://rdf.equinor.com/source/mel#Header53"),
            "BA498"
        );
    }

    private SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1);
    }

    private TransformationDetails CreateTransformationDetails()
    {
        var identitySegment = new TargetPathSegment("Tag Number", "test");
        return new TransformationDetails(DataUri,
         PredicateUri,
         identitySegment,
         new List<TargetPathSegment>()
            , RdfFormat.Turtle);
    }
}
