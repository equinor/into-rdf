using Common.RdfModels;
using System;
using VDS.RDF;

namespace Services.Tests
{
    internal class TestOntology
    {
        static readonly Graph ontologyGraph = new Graph();
        static INode subPropertyOf = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateSubPropertyOfProperty());
        static INode type = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateType());
        static INode range = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateRange());
        static INode domain = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateDomain());
        static INode label = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateLabel());
        static INode datatypeProperty = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateDatatypeProperty());
        static INode objectProperty = ontologyGraph.CreateUriNode(RdfCommonProperties.CreateObjectProperty());

        static INode owlClass = ontologyGraph.CreateUriNode(RdfCommonClasses.CreateOwlClass());
        static INode namedIndividual = ontologyGraph.CreateUriNode(RdfCommonClasses.CreateNamedIndividual());

        static INode doubleDatatype = ontologyGraph.CreateUriNode(RdfDatatypes.CreateDoubleDatatype());
        static INode stringDatatype = ontologyGraph.CreateUriNode(RdfDatatypes.CreateStringDatatype());

        static INode lineItem = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#LineItem"));
        static INode lineNumber = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#LineNumber"));
        static INode lineNumberSource = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/source/linelist#Line%20number"));
        static INode hasLineNumber = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasLineNumber"));

        static INode wallThickness = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#WallThickness"));
        static INode wallThicknessDatum = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#WallThicknessDatum"));
        static INode scale = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist#Scale"));

        static INode wallThicknessSource = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/source/linelist#Wall%20thk."));
        static INode hasWallThicknessQuantity = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#hasWallThicknessQuantity"));
        static INode hasPhysicalQuantity = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#hasPhysicalQuantity"));
        static INode wallThicknessQualifiedAs = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessQualifiedAs"));
        static INode quantityQualifiedAs = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#qualityQuantifiedAs"));
        static INode wallThicknessDatumValue = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessDatumValue"));
        static INode doubleDatumValue = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#datumValue"));
        static INode wallThicknessDatumUOM = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/linelist/v1#wallThicknessDatumUOM"));
        static INode datumUOM = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/physical/v1#datumUOM"));

        static INode millimeter = ontologyGraph.CreateUriNode(new Uri("https://rdf.equinor.com/ontology/uom/v1#millimeter"));
        static INode symbol = ontologyGraph.CreateUriNode(new Uri("http://www.ontology-of-units-of-measure.org/resource/om-2/symbol"));


        internal static Graph InitializeTestOntology()
        {
            ontologyGraph.Assert(new Triple(lineItem, type, owlClass));
            ontologyGraph.Assert(new Triple(lineNumber, type, owlClass));
            ontologyGraph.Assert(new Triple(hasLineNumber, subPropertyOf, datatypeProperty));
            ontologyGraph.Assert(new Triple(hasLineNumber, domain, lineItem));
            ontologyGraph.Assert(new Triple(hasLineNumber, range, stringDatatype));
            ontologyGraph.Assert(new Triple(lineNumberSource, subPropertyOf, hasLineNumber));

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
}
