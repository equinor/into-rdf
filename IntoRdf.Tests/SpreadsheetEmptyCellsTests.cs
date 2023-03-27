using IntoRdf.Public.Models;
using System;
using System.Collections.Generic;
using Xunit;
namespace IntoRdf.Tests;

public class SpreadsheetEmptyCellsTests
{
    private static readonly Uri DataUri = new Uri("http://example.com/");
    private static readonly Uri PredicateUri = new Uri("http://example.com/predicate#");
    private const string SheetName = "test";

    // For literal testing
    private static readonly TransformationDetails literalDetails = new TransformationDetails(DataUri, PredicateUri, null, new List<TargetPathSegment>(), RdfFormat.Turtle);
    private static readonly RdfTestUtil literalTester = new RdfTestUtil("TestData/emptycells.xlsx", CreateSpreadsheetDetails(), literalDetails);

    // For uri testing
    private static readonly TransformationDetails uriDetails = new TransformationDetails(DataUri, PredicateUri, null, new List<TargetPathSegment>
    {
        new TargetPathSegment ("data", "")
    }, RdfFormat.Turtle);
    private readonly RdfTestUtil uriTester = new RdfTestUtil("TestData/emptycells.xlsx", CreateSpreadsheetDetails(), uriDetails);
    private static bool written = false;

    [Fact]
    public void EmptyLiteralCells()
    {
        var idPredicate = "http://example.com/predicate#id";
        var dataPredicate = "http://example.com/predicate#data";
        var emptyPredicate = "http://example.com/predicate#Column1";

        var rowA = new Dictionary<string, object>
        {
            {idPredicate, "a" },
            {dataPredicate, "data a" },
        };

        var rowB = new Dictionary<string, object>
        {
            {idPredicate, "b" },
            {emptyPredicate, "empty b" },
        };

        literalTester.AssertObjectExist(rowA);
        literalTester.AssertObjectExist(rowB);
    }

    [Fact]
    public void EmptyUriCells()
    {
        if (!written)
        {
            // Console.WriteLine(uriTester.WriteGraphToString(RdfFormat.Turtle));
            written = true;
        }

        var idPredicate = "http://example.com/predicate#id";
        var dataPredicate = "http://example.com/predicate#data";
        var emptyPredicate = "http://example.com/predicate#Column1";

        var rowA = new Dictionary<string, object>
        {
            {idPredicate, "a" },
            {dataPredicate, new Uri("http://example.com/data%20a") },
        };

        var rowB = new Dictionary<string, object>
        {
            {idPredicate, "b" },
            {emptyPredicate, "empty b" },
        };

        uriTester.AssertObjectExist(rowA);
        uriTester.AssertObjectExist(rowB);
    }

    private static SpreadsheetDetails CreateSpreadsheetDetails()
    {
        return new SpreadsheetDetails(SheetName, 1, 2, 1) { EndColumn = int.MaxValue };
    }
}