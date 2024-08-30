using Xunit;
using IntoRdf.Models;

namespace IntoRdf.Nuget.Tests;

public class NugetTests
{
    private static readonly Uri DataUri = new("https://example.com/");
    private static readonly Uri PredicateUri = new("http://www.w3.org/ns/prov#");
    private const string SheetName = "sheetName";
    private readonly Utils _utils = new("TestData.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());

    [Fact]
    public void AssertProcessedData_AddsVersionToGraph()
    {
        // Arrange
        var predicateUri = new Uri("http://www.w3.org/ns/prov#wasAssociatedWith");
        var objectValue = "https://github.com/equinor/into-rdf/commit/unknown";

        //Act
        var triples = _utils.GetTriples();

        // Assert
        Assert.Contains(triples, t => t.Predicate.ToString() == predicateUri.ToString() && t.Object.ToString() == objectValue);
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1);
    }

    private static TransformationDetails CreateTransformationDetails()
    {
        return new TransformationDetails(
            DataUri,
            PredicateUri,
            null,
            [],
            RdfFormat.Turtle);
    }
}