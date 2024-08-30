using IntoRdf.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IntoRdf.Tests;

public class BasicTests
{
    private static readonly Uri DataUri = new Uri("https://rdf.equinor.com/");
    private static readonly Uri PredicateUri = new Uri("https://rdf.equinor.com/source/mel#");
    private const string SheetName = "sheetName";

    private readonly RdfTestUtil rdfTestUtils = new RdfTestUtil("TestData/basic.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());


    [Fact]
    public void NoTrailingSlash()
    {
        var subjects = rdfTestUtils.GetAllSubjects();
        Assert.All(subjects, (subject) => Assert.False(subject.EndsWith('/')));
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
}
