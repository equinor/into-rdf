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
    [InlineData("slash", "v/v", "v%2fv")]
    [InlineData("backslash", "v\\v", "v%5cv")]
    [InlineData("singleQuote", $"""v'v""", $"""v'v""")]
    [InlineData("doubleQuote", $"""v"v""", $"""v"v""")]
    [InlineData("leftParenthesis", "v(v", "v(v")]
    [InlineData("rightParanthesis", "v)v", "v)v")]
    [InlineData("space", "v v", "v v")]
    [InlineData("newLine", "v\nv", "v\nv")]
    [InlineData("tab", "v\tv", "v\tv")]
    [InlineData("tripleQuote", $""""v"""v"""", $""""v"""v"""")]
    public void EscapeValues(string name, string literalValue, string uriValue)
    {
        _valueTester.AssertTripleAsserted(name, "Value", literalValue);
        _uriTester.AssertTripleAsserted(uriValue, "Name", name);

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

