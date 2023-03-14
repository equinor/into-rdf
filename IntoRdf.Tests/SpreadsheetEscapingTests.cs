using IntoRdf.Public.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IntoRdf.Tests;

public class SpreadsheetEscapingTests
{
    private static readonly Uri DataUri = new Uri("http://example.com/");
    private static readonly Uri PredicateUri = new Uri("http://example.com/predicate/");
    private const string SheetName = "test";
    private readonly RdfTestUtil _valueTester = new RdfTestUtil("TestData/escaping.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails("Name"));
    private readonly RdfTestUtil _uriTester = new RdfTestUtil("TestData/escaping.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails("Value"));

    [Theory]
    [InlineData("slash", "v/v")]
    [InlineData("backslash", "v\\v")]
    [InlineData("singleQuote", $"""v'v""")]
    [InlineData("doubleQuote", $"""v"v""")]
    [InlineData("leftParenthesis", "v(v")]
    [InlineData("rightParanthesis", "v)v")]
    [InlineData("space", "v v")]
    [InlineData("newLine", "v\nv")]
    [InlineData("tab", "v\tv")]
    [InlineData("tripleQuote", $""""v"""v"""")]
    public void EscapeValues(string name, string value)
    {
        _valueTester.AssertTripleAsserted(name, "Value", value);
        _uriTester.AssertTripleAsserted(value, "Name", name);

    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1) { EndColumn = int.MaxValue };
    }

    private static TransformationDetails CreateTransformationDetails(string idColumn)
    {
        var segment = new TargetPathSegment(idColumn, "", true);
        return new TransformationDetails(DataUri, PredicateUri, new List<TargetPathSegment> { segment }, RdfFormat.Turtle);
    }
}

