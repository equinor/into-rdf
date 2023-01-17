/* using Common.ProvenanceModels;
using Services.TransformationServices.SourceToOntologyConversionService;
using Services.TransformationServices.RdfTransformationServices;
using Services.GraphParserServices;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using VDS.RDF;
using Xunit;

namespace Services.Tests;

public class LineListTests
{
    private readonly ISourceToOntologyConversionService _sourceVocabularyConversionService;
    private readonly IRdfTransformationService _transformationService;
    private readonly IGraphParser _graphParser;

    private readonly (string, double) tag1 = ("20L00015A", 4.23);
    private readonly (string, double) tag2 = ("20L00017A", 5.23);
    private readonly (string, double) tag3 = ("20L00018A", 6.28);

    private readonly (string, double) tagWithUnderscore = ("20L00015A", 1337.1);

    public LineListTests(ISourceToOntologyConversionService sourceVocabularyConversionService, 
        IRdfTransformationService transformationService,
        IGraphParser graphParser)
    {
        _sourceVocabularyConversionService = sourceVocabularyConversionService;
        _transformationService = transformationService;
        _graphParser = graphParser;
    }

    [Fact]
    public void TestDataWithUnderscore()
    {
        var rdfTestUtils = new RdfTestUtils(DataSource.LineList);
        var testData = new List<(string, object)> { tagWithUnderscore };
        var inputData = CreateTestDataForTag(testData);

        var testFile = "TestData/revisionTestTrainSiemens.ttl";
        string turtle = File.ReadAllText(testFile, System.Text.Encoding.Default);

        var revisionTrain = _graphParser.ParseRevisionTrain(turtle);

        var transformed = _transformationService.Transform(revisionTrain, inputData);

        var ontologyGraph = TestOntology.InitializeTestOntology();
        var converted = _sourceVocabularyConversionService.ConvertSourceToOntology(transformed, ontologyGraph);

        AssertCorrectDatumsForTestData(rdfTestUtils, converted, testData);
    }

    [Fact]
    public void TestSourceTermsToOntology()
    {
        var rdfTestUtils = new RdfTestUtils(DataSource.LineList);
        var testData = new List<(string, object)> { tag1, tag2, tag3 };
        var inputData = CreateTestDataForTag(testData);

        var testFile = "TestData/revisionTestTrainSiemens.ttl";
        string turtle = File.ReadAllText(testFile, System.Text.Encoding.Default);

        var revisionTrain = _graphParser.ParseRevisionTrain(turtle);

        var transformed = _transformationService.Transform(revisionTrain, inputData);

        var ontologyGraph = TestOntology.InitializeTestOntology();
        var converted = _sourceVocabularyConversionService.ConvertSourceToOntology(transformed, ontologyGraph);

        Console.WriteLine(GraphSupportFunctions.WriteGraphToString(converted));

        AssertCorrectDatumsForTestData(rdfTestUtils, converted, testData);
    }

    private void AssertCorrectDatumsForTestData(RdfTestUtils rdfTestUtils, Graph ontologyAnnotatedGraph, List<(string, object)> testData)
    {
        foreach ((string tag, object data) in testData)
        {
            var uri = "https://rdf.equinor.com/test/" + tag;
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
                tag
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

        inputData.Columns.Add(new DataColumn("id", typeof(string)));
        inputData.Columns.Add(new DataColumn("Line%20number", typeof(string)));
        inputData.Columns.Add(new DataColumn("Wall%20thk.", typeof(float)));

        foreach ((string tag, object data) in testData)
        {    
            DataRow row = inputData.NewRow();
            row[0] = tag;
            row[1] = tag;
            row[2] = data;
            inputData.Rows.Add(row);
        }
        return inputData;
    }
} */