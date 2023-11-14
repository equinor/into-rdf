using IntoRdf.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IntoRdf.Tests;

public class MissingSheetName
{
    private static readonly Uri DataUri = new Uri("https://rdf.equinor.com/");
    private static readonly Uri PredicateUri = new Uri("https://rdf.equinor.com/source/mel#");
    private const string SheetName = "sheetName";

    [Fact]
    public void ValidateSheet__ShouldNotify__WhenSheetNameIsNOtPresent()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new RdfTestUtil("C:\\Users\\johannes.telle\\source\\repos\\into-rdf\\IntoRdf.Tests\\TestData\\missingSheetName.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails()));

        // Assert that the exception message contains the expected string
        Assert.Contains("Did not find sheet with name sheetName among [Sheet1]", exception.Message);
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
