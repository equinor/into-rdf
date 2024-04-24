using System;
using System.Collections.Generic;
using System.Reflection;
using IntoRdf.Models;
using Xunit;

namespace IntoRdf.Tests;

public class AddIntoRdfVersionTest
{
    private static readonly Uri DataUri = new("https://example.com/");
    private static readonly Uri PredicateUri = new("https://example.com/");
    private const string SheetName = "sheetName";

    private readonly RdfTestUtil _rdfTestUtils = new("TestData/testAddIntoRdfVersion.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());

    [Fact]
    public void AssertProcessedData_AddsVersionToGraph()
    {
        // Arrange
        const string subjectSuffix = "into-rdf";
        const string predicateSuffix = "hasVersion";
        var objectValue = GetIntoRdfVersion();

        // Act & Assert
        _rdfTestUtils.AssertTripleAsserted(subjectSuffix, predicateSuffix, objectValue);
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