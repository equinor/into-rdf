using Common.ProvenanceModels;
using Common.RdfModels;
using Services.TransformationServices.SourceToOntologyConversionService;
using System;
using System.Data;
using VDS.RDF;
using Xunit;

namespace Services.Tests;

public class LineListTests
{
    public readonly ISourceToOntologyConversionService _sourceVocabularyConversionService;

    public LineListTests(ISourceToOntologyConversionService sourceVocabularyConversionService)
    {
        _sourceVocabularyConversionService = sourceVocabularyConversionService;
    }

    [Fact]
    public void TestSourceTermsToOntology()
    {
        var rdfTestUtils = new RdfTestUtils(DataSource.LineList);
        var inputData = InitializeTestData();
        var ontologyGraph = InitializeTestOntology();

        _sourceVocabularyConversionService.ConvertSourceToOntology(inputData, ontologyGraph);
        Graph ontologyAnnotatedGraph = _sourceVocabularyConversionService.GetGraph();

        Assert.NotNull(ontologyAnnotatedGraph);

        rdfTestUtils.AssertTripleAsserted(
            ontologyAnnotatedGraph,
            new Uri("https://rdf.equinor.com/test/linelist/20L00018A_WallThicknessDatum"),
            new Uri("https://rdf.equinor.com/ontology/physical/v1#datumValue"),
            4.19
         );

        rdfTestUtils.AssertTripleAsserted(
            ontologyAnnotatedGraph,
            new Uri("https://rdf.equinor.com/test/linelist/20L00015A"),
            new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasLineNumber"),
            "20L00015A"
        );

        rdfTestUtils.AssertTripleAsserted(
            ontologyAnnotatedGraph,
            new Uri("https://rdf.equinor.com/test/linelist/20L00015A"),
            new Uri("https://rdf.equinor.com/ontology/physical/v1#hasPhysicalQuantity"),
            new Uri("https://rdf.equinor.com/test/linelist/20L00015A_WallThickness")
        );

        rdfTestUtils.AssertTripleAsserted(
            ontologyAnnotatedGraph,
            new Uri("https://rdf.equinor.com/test/linelist/20L00015A_WallThickness"),
            new Uri("https://rdf.equinor.com/ontology/physical/v1#qualityQuantifiedAs"),
            new Uri("https://rdf.equinor.com/test/linelist/20L00015A_WallThicknessDatum")
        );

        rdfTestUtils.AssertTripleAsserted(
            ontologyAnnotatedGraph,
            new Uri("https://rdf.equinor.com/test/linelist/20L00018A_WallThicknessDatum"),
            new Uri("https://rdf.equinor.com/ontology/physical/v1#datumUOM"),
            new Uri("https://rdf.equinor.com/ontology/uom/v1#millimeter")
        );
    }

    private DataTable InitializeTestData()
    {
        DataTable inputData = new DataTable();

        inputData.Columns.Add(new DataColumn("id", typeof(Uri)));
        inputData.Columns.Add(new DataColumn("https://rdf.equinor.com/source/linelist#Line%20number", typeof(string)));
        inputData.Columns.Add(new DataColumn("https://rdf.equinor.com/source/linelist#Wall%20thk.", typeof(float)));

        DataRow row = inputData.NewRow();

        row[0] = new Uri("https://rdf.equinor.com/test/linelist/20L00015A");
        row[1] = "20L00015A";
        row[2] = 14.27;
        inputData.Rows.Add(row);

        row = inputData.NewRow();
        row[0] = new Uri("https://rdf.equinor.com/test/linelist/20L00017A");
        row[1] = "20L00017A";
        row[2] = 6.35;
        inputData.Rows.Add(row);

        row = inputData.NewRow();
        row[0] = new Uri("https://rdf.equinor.com/test/linelist/20L00018A");
        row[1] = "20L00018A";
        row[2] = 4.19;
        inputData.Rows.Add(row);

        return inputData;
    }

    private Graph InitializeTestOntology()
    {
        Graph ontologyGraph = new Graph();
        var subPropertyOf = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateSubPropertyOfProperty());
        var type = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateType());
        var range = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateRange());
        var domain = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateDomain());
        var label = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateLabel());
        var datatypeProperty = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateDatatypeProperty());
        var objectProperty = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateObjectProperty());

        var owlClass = ontologyGraph.CreateUriNode(RdfCommonClasses.CreateOwlClass());
        var namedIndividual = ontologyGraph.CreateUriNode(RdfCommonClasses.CreateNamedIndividual());

        var uriDatatype = ontologyGraph.CreateUriNode(RdfDatatypes.CreateUriDatatype());
        var doubleDatatype = ontologyGraph.CreateUriNode(RdfDatatypes.CreateDoubleDatatype());
        var stringDatatype = ontologyGraph.CreateUriNode(RdfDatatypes.CreateStringDatatype());

        var lineItem = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#LineItem"));
        var lineNumber = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#LineNumber"));
        var lineNumberSource = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/source/linelist#Line%20number"));
        var hasLineNumber = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasLineNumber"));

        ontologyGraph.Assert(new Triple(lineItem, type, owlClass));
        ontologyGraph.Assert(new Triple(lineNumber, type, owlClass));
        ontologyGraph.Assert(new Triple(hasLineNumber, subPropertyOf, datatypeProperty));
        ontologyGraph.Assert(new Triple(hasLineNumber, domain, lineItem));
        ontologyGraph.Assert(new Triple(hasLineNumber, range, stringDatatype));
        ontologyGraph.Assert(new Triple(lineNumberSource, subPropertyOf, hasLineNumber));

        var wallThickness = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#WallThickness"));
        var wallThicknessDatum = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#WallThicknessDatum"));
        var scale = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#Scale"));

        var wallThicknessSource = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/source/linelist#Wall%20thk."));
        var hasWallThicknessQuantity = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasWallThicknessQuantity"));
        var hasPhysicalQuantity = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#hasPhysicalQuantity"));
        var wallThicknessQualifiedAs = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessQualifiedAs"));
        var quantityQualifiedAs = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#qualityQuantifiedAs"));
        var wallThicknessDatumValue = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessDatumValue"));
        var doubleDatumValue = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#datumValue"));
        var wallThicknessDatumUOM = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessDatumUOM"));
        var datumUOM = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#datumUOM"));

        var millimeter = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/uom/v1#millimeter"));
        var symbol = ontologyGraph.CreateUriNode(new Uri("http://www.ontology-of-units-of-measure.org/resource/om-2/symbol"));

        ontologyGraph.Assert(new Triple(wallThickness, type, owlClass));
        ontologyGraph.Assert(new Triple(wallThicknessDatum, type, owlClass));
        ontologyGraph.Assert(new Triple(millimeter, type, namedIndividual));
        ontologyGraph.Assert(new Triple(millimeter, type, scale));
        ontologyGraph.Assert(new Triple(millimeter, label, ontologyGraph.CreateLiteralNode("millimeter")));
        ontologyGraph.Assert(new Triple(millimeter, symbol, ontologyGraph.CreateLiteralNode("mm")));

        ontologyGraph.Assert(new Triple(wallThicknessSource, subPropertyOf, hasWallThicknessQuantity));
        ontologyGraph.Assert(new Triple(hasWallThicknessQuantity, subPropertyOf, hasPhysicalQuantity));
        ontologyGraph.Assert(new Triple(hasWallThicknessQuantity, domain, lineItem));
        ontologyGraph.Assert(new Triple(hasWallThicknessQuantity, range, wallThickness));

        ontologyGraph.Assert(new Triple(wallThicknessQualifiedAs, subPropertyOf, quantityQualifiedAs));
        ontologyGraph.Assert(new Triple(wallThicknessQualifiedAs, domain, wallThickness));
        ontologyGraph.Assert(new Triple(wallThicknessQualifiedAs, range, wallThicknessDatum));

        ontologyGraph.Assert(new Triple(doubleDatumValue, subPropertyOf, datatypeProperty));
        ontologyGraph.Assert(new Triple(wallThicknessDatumValue, subPropertyOf, doubleDatumValue));
        ontologyGraph.Assert(new Triple(wallThicknessDatumValue, domain, wallThicknessDatum));
        ontologyGraph.Assert(new Triple(wallThicknessDatumValue, range, doubleDatatype));

        ontologyGraph.Assert(new Triple(datumUOM, subPropertyOf, objectProperty));
        ontologyGraph.Assert(new Triple(wallThicknessDatumUOM, subPropertyOf, datumUOM));
        ontologyGraph.Assert(new Triple(wallThicknessDatumUOM, domain, wallThicknessDatum));
        ontologyGraph.Assert(new Triple(wallThicknessDatumUOM, range, millimeter));

        return ontologyGraph;
    }
}