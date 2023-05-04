using IntoRdf.Models;
using System;
using System.Collections.Generic;
using Xunit;
namespace IntoRdf.Tests;

public class SpreadsheetSelectingRowTests
{
    private static readonly Uri DataUri = new Uri("http://example.com/");
    private static readonly Uri PredicateUri = new Uri("http://example.com/predicate#");
    private const string RowSheetName = "EmptyRows";
    private const string ColumnSheetName = "EmptyColumns";

    private static readonly TransformationDetails transformationDetails = new TransformationDetails(DataUri, PredicateUri, null, new List<TargetPathSegment>(), RdfFormat.Turtle);
    private static readonly RdfTestUtil selectEmptyRowTester = new RdfTestUtil("TestData/emptyColumnsAndRows.xlsx", new SpreadsheetDetails(RowSheetName, 5, 9, 1), transformationDetails);
    private static readonly RdfTestUtil selectEmptyColumnTester = new RdfTestUtil("TestData/emptyColumnsAndRows.xlsx", new SpreadsheetDetails(ColumnSheetName, 1, 2, 1), transformationDetails);

    private static SpreadsheetDetails endRowDetails = new SpreadsheetDetails(RowSheetName, 5, 9, 1) { DataEndRow = 9 };
    private static readonly RdfTestUtil selectEndRowTester = new RdfTestUtil("TestData/emptyColumnsAndRows.xlsx", endRowDetails, transformationDetails);

    [Fact]
    public void SelectWithEmptyRows()
    {
        var header1Predicate = "http://example.com/predicate#Header1";
        var header2Predicate = "http://example.com/predicate#Header2";
        var header3Predicate = "http://example.com/predicate#Header3";
        var header4Predicate = "http://example.com/predicate#Header4";

        var rowA = new Dictionary<string, object>
        {
            {header1Predicate, "Data1_1"},
            {header2Predicate, "Data1_2"},
            {header3Predicate, "Data1_3"},
            {header4Predicate, "Data1_4"},
        };

        var rowB = new Dictionary<string, object>
        {
            {header1Predicate, "Data2_1"},
            {header2Predicate, "Data2_2"},
            {header3Predicate, "Data2_3"},
            {header4Predicate, "Data2_4"},
        };

        selectEmptyRowTester.AssertObjectExist(rowA);
        selectEmptyRowTester.AssertObjectExist(rowB);
        selectEmptyRowTester.AssertTripleCount(8);
    }

    [Fact]
    public void SelectWithEndRows()
    {
        var header1Predicate = "http://example.com/predicate#Header1";
        var header2Predicate = "http://example.com/predicate#Header2";
        var header3Predicate = "http://example.com/predicate#Header3";
        var header4Predicate = "http://example.com/predicate#Header4";

        var rowA = new Dictionary<string, object>
        {
            {header1Predicate, "Data1_1"},
            {header2Predicate, "Data1_2"},
            {header3Predicate, "Data1_3"},
            {header4Predicate, "Data1_4"},
        };

        selectEndRowTester.AssertObjectExist(rowA);
        selectEndRowTester.AssertTripleCount(4);
    }

    [Fact]
    public void SelectWithEmptyColumns()
    {
        var header1Predicate = "http://example.com/predicate#Header1";
        var header2Predicate = "http://example.com/predicate#Header2";
        var header3Predicate = "http://example.com/predicate#Header3";
        var header4Predicate = "http://example.com/predicate#Header4";

        var rowA = new Dictionary<string, object>
        {
            {header1Predicate, "Data1"},
            {header2Predicate, "Data2"},
            {header3Predicate, "Data3"},
            {header4Predicate, "Data4"},
        };

        selectEmptyColumnTester.AssertObjectExist(rowA);
        selectEmptyColumnTester.AssertTripleCount(4);
    }
}