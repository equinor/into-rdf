using Common.ProvenanceModels;
using Services.TransformationServices.SourceToOntologyConversionService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VDS.RDF;
using Xunit;

namespace Services.Tests;

public class LineListTests
{
    private readonly ISourceToOntologyConversionService _sourceVocabularyConversionService;

    private readonly (string, double) tag1 = ("https://rdf.equinor.com/test/linelist/20L00015A", 4.23);
    private readonly (string, double) tag2 = ("https://rdf.equinor.com/test/linelist/20L00017A", 5.23);
    private readonly (string, double) tag3 = ("https://rdf.equinor.com/test/linelist/20L00018A", 6.28);

    private readonly (string, double) tagWithUnderscore = ("https://rdf.equinor.com/test/line_list/20L00015A", 1337.1);

    public LineListTests(ISourceToOntologyConversionService sourceVocabularyConversionService)
    {
        _sourceVocabularyConversionService = sourceVocabularyConversionService;
    }

    [Fact]
    public void TestDataWithUnderscore()
    {
        var rdfTestUtils = new RdfTestUtils(DataSource.LineList);
        var testData = new List<(string, object)> { tagWithUnderscore };
        var inputData = CreateTestDataForTag(testData);

        var ontologyGraph = TestOntology.InitializeTestOntology();
        _sourceVocabularyConversionService.ConvertSourceToOntology(inputData, ontologyGraph);

        AssertCorrectDatumsForTestData(rdfTestUtils, _sourceVocabularyConversionService.GetGraph(), testData);
    }

    [Fact]
    public void TestSourceTermsToOntology()
    {
        var rdfTestUtils = new RdfTestUtils(DataSource.LineList);
        var testData = new List<(string, object)> { tag1, tag2, tag3 };
        var inputData = CreateTestDataForTag(testData);

        var ontologyGraph = TestOntology.InitializeTestOntology();
        _sourceVocabularyConversionService.ConvertSourceToOntology(inputData, ontologyGraph);

        AssertCorrectDatumsForTestData(rdfTestUtils, _sourceVocabularyConversionService.GetGraph(), testData);
    }

    private void AssertCorrectDatumsForTestData(RdfTestUtils rdfTestUtils, Graph ontologyAnnotatedGraph, List<(string, object)> testData)
    {
        foreach ((string uri, object data) in testData)
        {
            rdfTestUtils.AssertTripleAsserted(
                ontologyAnnotatedGraph,
                new Uri($"{uri}_WallThicknessDatum"),
                new Uri("https://rdf.equinor.com/ontology/physical/v1#datumValue"),
                data
             );

            rdfTestUtils.AssertTripleAsserted(
                ontologyAnnotatedGraph,
                new Uri(uri),
                new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasLineNumber"),
                TagFromUri(uri)
            );

            rdfTestUtils.AssertTripleAsserted(
                ontologyAnnotatedGraph,
                new Uri(uri),
                new Uri("https://rdf.equinor.com/ontology/physical/v1#hasPhysicalQuantity"),
                new Uri($"{uri}_WallThickness")
            );

            rdfTestUtils.AssertTripleAsserted(
                ontologyAnnotatedGraph,
                new Uri($"{uri}_WallThickness"),
                new Uri("https://rdf.equinor.com/ontology/physical/v1#qualityQuantifiedAs"),
                new Uri($"{uri}_WallThicknessDatum")
            );

            rdfTestUtils.AssertTripleAsserted(
                ontologyAnnotatedGraph,
                new Uri($"{uri}_WallThicknessDatum"),
                new Uri("https://rdf.equinor.com/ontology/physical/v1#datumUOM"),
                new Uri("https://rdf.equinor.com/ontology/uom/v1#millimeter")
            );
        }
    }

    private DataTable CreateTestDataForTag(List<(string, object)> testData)
    {
        DataTable inputData = new DataTable();

        inputData.Columns.Add(new DataColumn("id", typeof(Uri)));
        inputData.Columns.Add(new DataColumn("https://rdf.equinor.com/source/linelist#Line%20number", typeof(string)));
        inputData.Columns.Add(new DataColumn("https://rdf.equinor.com/source/linelist#Wall%20thk.", typeof(float)));

        foreach ((string uri, object data) in testData)
        {
            DataRow row = inputData.NewRow();
            row[0] = new Uri(uri);
            row[1] = TagFromUri(uri);
            row[2] = data;
            inputData.Rows.Add(row);
        }
        return inputData;
    }

    private string TagFromUri(string uri)
    {
        return uri.ToString().Split("/").Last();
    }
}