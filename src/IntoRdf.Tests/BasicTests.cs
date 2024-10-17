using IntoRdf.Models;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Xunit;

namespace IntoRdf.Tests;

public class BasicTests
{
    private static readonly Uri DataUri = new Uri("https://rdf.equinor.com/");
    private static readonly Uri PredicateUri = new Uri("https://rdf.equinor.com/source/mel#");
    private const string SheetName = "sheetName";
    private const string RdfsLabel = "http://www.w3.org/2000/01/rdfschema#label";



    [Fact]
    public void NoTrailingSlash()
    {
        RdfTestUtil rdfTestUtils = new RdfTestUtil("TestData/basic.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails());
        var subjects = rdfTestUtils.GetAllSubjects();
        Assert.All(subjects, (subject) => Assert.False(subject.EndsWith('/')));
    }

    [Fact]
    public void CreateRdfsLabel()
    {
        RdfTestUtil rdfTestUtils = new RdfTestUtil("TestData/basic.xlsx", CreateSpreadsheetDetails(), CreateLabelTransformationDetails());
        rdfTestUtils.AssertObjectExist(new Dictionary<string, object> {{RdfsLabel, "Donatello"}}, true);
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1);
    }

    private static TransformationDetails CreateTransformationDetails()
    {
        return new TransformationDetails(DataUri, PredicateUri, null, [], RdfFormat.Turtle);
    }

    private static TransformationDetails CreateLabelTransformationDetails()
    {
        var rdfsLabelConfig = new TargetPathSegment ("Name", null, RdfsLabel);
        return new TransformationDetails(DataUri, PredicateUri, null, [rdfsLabelConfig], RdfFormat.Turtle);
    }
}
