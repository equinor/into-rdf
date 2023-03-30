using IntoRdf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IntoRdf.Tests;

public class SpreadsheetEscapingTests
{
    private static readonly Uri DataUri = new Uri("http://example.com/");
    private static readonly Uri PredicateUri = new Uri("http://example.com/predicate#");
    private const string SheetName = "test";
    private readonly RdfTestUtil _valueTester = new RdfTestUtil("TestData/escaping.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails("Name"));
    private readonly RdfTestUtil _idUriTester = new RdfTestUtil("TestData/escaping.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails("Value"));
    private readonly RdfTestUtil _otherUriTester = new RdfTestUtil("TestData/escaping.xlsx", CreateSpreadsheetDetails(), CreateTransformationDetails("Name", "Value"));
    private static bool written = false;

    [Theory]
    [InlineData("slash", "v/v", "v%2Fv")]
    [InlineData("backslash", "v\\v", "v%5Cv")]
    [InlineData("singleQuote", "v'v", "v%27v")]
    [InlineData("doubleQuote", $"""v"v""", $"""v"v""")]
    [InlineData("leftParenthesis", "v(v", "v%28v")]
    [InlineData("rightParanthesis", "v)v", "v%29v")]
    [InlineData("space", "v v", "v%20v")]
    [InlineData("newLine", "v\nv", "v\nv")]
    [InlineData("tab", "v\tv", "v\tv")]
    [InlineData("tripleQuote", $""""v"""v"""", $""""v"""v"""")]
    [InlineData("dot", "v.v", "v.v")]
    [InlineData("comma", "v,v", "v%2Cv")]
    [InlineData("trailing1", "vv1", "vv1")]
    [InlineData("trailing2", "vv2", "vv2")]
    [InlineData("exclamation", "v!v", "v%21v")]
    [InlineData("question", "v?v", "v%3Fv")]
    [InlineData("doubleQuestion", "v?v?v", "v%3Fv%3Fv")]
    [InlineData("plus", "v+v", "v%2Bv")]
    [InlineData("minus", "v-v", "v-v")]
    [InlineData("star", "v*v", "v%2Av")]
    [InlineData("hash", "v#v", "v%23v")]
    [InlineData("doubleHash", "v#v#v", "v%23v%23v")]
    [InlineData("percent", "v%v", "v%25v")]
    [InlineData("ampersand", "v&v", "v%26v")]
    [InlineData("equal", "v=v", "v%3Dv")]
    [InlineData("at", "v@v", "v%40v")]
    [InlineData("colon", "v:v", "v%3Av")]
    [InlineData("gt", "v>v", "v>v")]
    [InlineData("lt", "v<v", "v<v")]
    [InlineData("pipe", "v|v", "v%7Cv")]
    [InlineData("semicolon", "v;v", "v%3Bv")]
    public void Escaping(string name, string literalValue, string uriValue)
    {

        _valueTester.AssertTripleAsserted(name, "Value", literalValue);
        _idUriTester.AssertTripleAsserted(uriValue, "Name", name);
        _otherUriTester.AssertTripleAsserted(new Uri(DataUri, name), new Uri($"{PredicateUri}Value"), new Uri(DataUri, $"Value/{uriValue}"));
        if (!written) {
            //Console.WriteLine(_otherUriTester.WriteGraphToString(RdfFormat.Turtle));
            written = true;
        }
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1) { EndColumn = int.MaxValue };
    }

    private static TransformationDetails CreateTransformationDetails(params string[] uriColumn)
    {
        var idSegment = new TargetPathSegment(uriColumn[0], "");
        var otherSegments = uriColumn.Skip(1).Select(segmentName => new TargetPathSegment(segmentName, segmentName)).ToList();
        return new TransformationDetails(DataUri, PredicateUri,idSegment, otherSegments, RdfFormat.Turtle);
    }
}