using System;
using System.Collections.Generic;
using System.Reflection;
using IntoRdf.Models;
using Xunit;

namespace IntoRdf.Tests;

public class AddIntoRdfVersionTest
{
    private static readonly Uri DataUri = new("https://example.com/");
    private static readonly Uri PredicateUri = new("http://www.w3.org/ns/prov#");
    private const string SheetName = "sheetName";

    private readonly RdfTestUtil _rdfTestUtils = new("TestData/testAddIntoRdfVersion.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());

    [Fact]
    public void AssertProcessedData_AddsVersionToGraph()
    {
        // Arrange
        var predicateUri = new Uri("http://www.w3.org/ns/prov#wasAssociatedWith");
        var objectValue = "https://github.com/equinor/into-rdf/commit/unknown";

        //Act
        var triples = _rdfTestUtils.GetTriples();

        // Assert
        Assert.Contains(triples, t => t.Predicate.ToString() == predicateUri.ToString() && t.Object.ToString() == objectValue);
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1);
    }

    private static TransformationDetails CreateTransformationDetails()
    {
        return new TransformationDetails(DataUri,
            PredicateUri,
            null,
            new List<TargetPathSegment>()
            , RdfFormat.Turtle);
    }

    private static string GetIntoRdfVersion()
    {
        var assembly = Assembly.Load("IntoRdf");
        return assembly.GetName().Version?.ToString();
    }
}