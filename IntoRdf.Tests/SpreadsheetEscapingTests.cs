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
    private static bool written = false;

    [Theory]
    [InlineData("slash", "v/v", "v%2fv")]
    [InlineData("backslash", "v\\v", "v%5cv")]
    [InlineData("singleQuote", "v'v", "v'v")]
    [InlineData("doubleQuote", $"""v"v""", $"""v"v""")]
    [InlineData("leftParenthesis", "v(v", "v(v")]
    [InlineData("rightParanthesis", "v)v", "v)v")]
    [InlineData("space", "v v", "v v")]
    [InlineData("newLine", "v\nv", "v\nv")]
    [InlineData("tab", "v\tv", "v\tv")]
    [InlineData("tripleQuote", $""""v"""v"""", $""""v"""v"""")]
    [InlineData("dot", "v.v", "v.v")]
    [InlineData("comma", "v,v", "v,v")]
    [InlineData("trailing1", "vv1 ", "vv1")]
    [InlineData("trailing2", "vv2  ", "vv2")]
    [InlineData("exclamation", "v!v", "v!v")]
    [InlineData("question", "v?v", "v?v")]
    [InlineData("doubleQuestion", "v?v?v", "v?v?v")]
    [InlineData("plus", "v+v", "v+v")]
    [InlineData("minus", "v-v", "v-v")]
    [InlineData("star", "v*v", "v*v")]
    [InlineData("hash", "v#v", "v%23v")]
    [InlineData("doubleHash", "v#v#v", "v%23v%23v")]
    [InlineData("percent", "v%v", "v%25v")]
    [InlineData("ampersand", "v&v", "v&v")]
    [InlineData("equal", "v=v", "v=v")]
    [InlineData("at", "v@v", "v@v")]
    [InlineData("colon", "v:v", "v%3av")]
    [InlineData("gt", "v>v", "v>v")]
    [InlineData("lt", "v<v", "v<v")]
    [InlineData("pipe", "v|v", "v%7cv")]
    [InlineData("semicolon", "v;v", "v;v")]
    public void Escaping(string name, string literalValue, string uriValue)
    {
        _valueTester.AssertTripleAsserted(name, "Value", literalValue);
        _uriTester.AssertTripleAsserted(uriValue, "Name", name);
        if (!written) {
            // Console.WriteLine(_uriTester.WriteGraphToString(RdfFormat.Turtle));
            written = true;
        }
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1) { EndColumn = int.MaxValue };
    }

    private static TransformationDetails CreateTransformationDetails(string idColumn)
    {
        var segment = new TargetPathSegment("idColumn", "");
        return new TransformationDetails(DataUri, PredicateUri,segment, new List<TargetPathSegment>(), RdfFormat.Turtle);
    }
}