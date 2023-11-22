using IntoRdf.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IntoRdf.Tests;

public class EmptyFormulaCellstest
{
    private static readonly Uri DataUri = new Uri("https://rdf.equinor.com/");
    private static readonly Uri PredicateUri = new Uri("https://rdf.equinor.com/source/mel#");
    private const string SheetName = "sheetName";

    [Fact]
    public void ValidateSheet__ShouldNotify__WhenSheetContainsEmptyFormulas()
    {
        var exception = Assert.Throws<Exception>(() => new RdfTestUtil("TestData/EmptyFormulaCells.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails()));

        // Assert that the exception message contains the expected string
        Assert.Contains("The cell at position(s) C6, C7 contains a formula but has no value.", exception.Message);
    }

    private SpreadsheetDetails CreateSpreadsheetDetails()
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
}
