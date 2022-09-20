using Common.RdfModels;
using Microsoft.Extensions.Logging;
using System.Data;
using VDS.RDF;
using VDS.RDF.Query.Builder;

namespace Services.TransformationServices.SourceToOntologyConversionService;

public class SourceToOntologyConversionService : ISourceToOntologyConversionService
{
    private Graph _graph;
    private ILogger<SourceToOntologyConversionService> _logger;

    public SourceToOntologyConversionService(ILogger<SourceToOntologyConversionService> logger)
    {
        _graph = InitializeGraph();
        _logger = logger;
    }

    private Graph InitializeGraph()
    {
        var graph = new Graph();
        foreach (var pair in RdfPrefixes.Prefix2Uri)
        {
            graph.NamespaceMap.AddNamespace(pair.Key, pair.Value);
        }

        return graph;
    }

    public void ConvertSourceToOntology(DataTable data, Graph ontologyGraph)
    {
        var ontologyNamespaces = GetOntologyNamespace(ontologyGraph);

        foreach (var ns in ontologyNamespaces)
        {
            _graph.NamespaceMap.AddNamespace(ns.Key, new Uri(ns.Value));
        }

        foreach (DataRow row in data.Rows)
        {
            var rowIndividual = _graph.CreateUriNode((Uri)row["id"]);
            AssertOwlNamedIndividual(rowIndividual);

            foreach (DataColumn header in data.Columns)
            {
                if (header.ColumnName == "id" || IsNull(row[header]))
                {
                    continue;
                }

                var cellValue = row[header]?.ToString() ?? "";
                var headerPredicate = ontologyGraph.CreateUriNode(new Uri(header.ToString()));

                //Normally, header predicates only have a single super predicate, but we want to allow for several.
                //Example: <https://rdf.equinor.com/source/linelist#Wall%20thk.> rdfs:subPropertyOf :hasWallThicknessQuantity .
                var superPredicates = GetSuperPredicates(ontologyGraph, headerPredicate);

                foreach (var superPredicate in superPredicates)
                {
                    TraversePredicate(ontologyGraph, rowIndividual, superPredicate, cellValue);
                }
            }
        }
    }

    public Graph GetGraph()
    {
        return _graph;
    }

    private void TraversePredicate(Graph ontologyGraph, INode subjectIndividual, INode predicate, string cellValue)
    {
        if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateHasPhysicalQuantity()))
        {
            var rangesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);
            foreach (var rangeOfPredicate in rangesOfPredicate)
            {
                AssertRangesAsCustomProperty(ontologyGraph, subjectIndividual, rangeOfPredicate, _graph.CreateUriNode(RdfCommonProperties.CreateHasPhysicalQuantity()));

                TraversePredicatesWithDomain(ontologyGraph, subjectIndividual, rangeOfPredicate, cellValue);
            }
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateQuantityQualifiedAs()))
        {
            var rangesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);
            foreach (var rangeOfPredicate in rangesOfPredicate)
            {
                AssertRangesAsCustomProperty(ontologyGraph, subjectIndividual, rangeOfPredicate, _graph.CreateUriNode(RdfCommonProperties.CreateQuantityQualifiedAs()));

                TraversePredicatesWithDomain(ontologyGraph, subjectIndividual, rangeOfPredicate, cellValue);
            }
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatumUOM()))
        {
            AssertUomProperty(ontologyGraph, subjectIndividual, predicate);
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatumValue()))
        {
            AssertDatumValue(ontologyGraph, subjectIndividual, predicate, cellValue);
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateObjectProperty()))
        {
            //TODO - TO BE IMPLEMENTED WHEN KRAFLA LINE LIST IS COVERED BY ONTOLOGY 
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatatypeProperty()))
        {
            AssertValue(ontologyGraph, subjectIndividual, predicate, cellValue);
        }
    }

    private IEnumerable<INode> GetRangesFromPredicate(Graph ontologyGraph, INode predicate)
    {
        var rangeTypes = ontologyGraph.GetTriplesWithSubject(predicate)
            .Where(t => t.Predicate.ToString() == RdfCommonProperties.CreateRange().ToString())
            .Select(t => t.Object);

        return rangeTypes;
    }

    private INode CreateIndividualWithTypeSuffix(Graph ontologyGraph, INode individual, INode individualType)
    {
        var individualBaseUri = GetBaseUri(individual);
        var suffix = GetUriFragment(individualType);

        return _graph.CreateUriNode(new Uri($"{individualBaseUri}_{suffix}"));
    }

    private bool HasSuperPredicate(Graph ontologyGraph, INode predicate, Uri requestedSuperPredicate)
    {
        var hasSuperPredicate = false;
        var superPredicates = GetSuperPredicates(ontologyGraph, predicate);

        foreach (var superPredicate in superPredicates)
        {
            if (superPredicate.ToString() == requestedSuperPredicate.AbsoluteUri)
            {
                return true;
            }

            //If any part of the predicate tree returns true, then the recursion returns true.
            hasSuperPredicate = hasSuperPredicate ? hasSuperPredicate : HasSuperPredicate(ontologyGraph, superPredicate, requestedSuperPredicate);
        }

        return hasSuperPredicate;
    }

    private IEnumerable<INode> GetSuperPredicates(Graph ontologyGraph, INode predicate)
    {
        var superPredicates = ontologyGraph.GetTriplesWithSubject(predicate)
            .Where(t => t.Predicate.ToString() == RdfCommonProperties.CreateSubPropertyOfProperty().AbsoluteUri)
            .Select(t => t.Object);

        return superPredicates;
    }

    private IEnumerable<INode> GetPredicatesWithDomain(Graph ontologyGraph, INode domain)
    {
        var predicatesWithDomain = ontologyGraph.GetTriplesWithObject(domain)
            .Where(t => t.Predicate.ToString() == RdfCommonProperties.CreateDomain().ToString())
            .Select(t => t.Subject);

        return predicatesWithDomain;
    }

    private void TraversePredicatesWithDomain(Graph ontologyGraph, INode subjectIndividual, INode domainObject, string cellValue)
    {
        var nextSubjectIndividual = CreateIndividualWithTypeSuffix(ontologyGraph, subjectIndividual, domainObject);

        // The domainObject, is typically the range of a previously visited predicate, and the domain domain of one or several other predicates.
        // For instance:
        // :wallThicknessQuantifiedAs rdfs:range physical:WallThicknessDatum ; 
        // :wallThicknessDatumValue rdfs:domain physical:WallThicknessDatum ;
        // The transformation traverse the ontology by exploring the range/domain objects of predicates. 
        var predicatesWithDomain = GetPredicatesWithDomain(ontologyGraph, domainObject);

        foreach (var predicateWithDomain in predicatesWithDomain)
        {
            TraversePredicate(ontologyGraph, nextSubjectIndividual, predicateWithDomain, cellValue);
        }
    }

    //TODO - Fix handling of underscore in uri. The base uri is mainly retrieved from individuals created by us, and these do not contain underscore.
    //But things tend to change. 
    private string GetBaseUri(INode node)
    {
        var baseIndividualIndex = node.ToString().IndexOf("_") == -1 ? node.ToString().Length : node.ToString().IndexOf("_");
        return node.ToString().Substring(0, baseIndividualIndex);
    }

    private string GetUriFragment(INode node)
    {
        return node.ToString().Split("#")[1];
    }

    private void AssertOwlNamedIndividual(INode individual)
    {
        AssertTriple(individual, _graph.CreateUriNode(RdfCommonProperties.CreateType()), _graph.CreateUriNode(RdfCommonClasses.CreateNamedIndividual()));
    }

    private void AssertOwlClass(INode individual, INode owlClass)
    {
        AssertTriple(individual, _graph.CreateUriNode(RdfCommonProperties.CreateType()), owlClass);
    }

    private void AssertProperty(INode subjectIndividual, INode predicate, INode rangeIndividual)
    {
        AssertTriple(subjectIndividual, predicate, rangeIndividual);
    }

    private void AssertRangesAsCustomProperty(Graph ontologyGraph, INode subjectIndividual, INode rangeOfPredicate, INode customPredicate)
    {
        // New individuals are created as the ontology is traversed. 
        // The subject individual is the current individual                                         :  <https://rdf.equinor.com/wist/c277/20L00015A_WallThickness>
        // The rangeOfPredicate is a type given by the range relationship of a predicate.           :  <https://rdf.equinor.com/ontology/physical/v1#WallThicknessDatum>
        // The object individual is the individual created as a combination of the previous two     :  <https://rdf.equinor.com/wist/c277/20L00015A_WallThicknessDatum>
        var objectIndividual = CreateIndividualWithTypeSuffix(ontologyGraph, subjectIndividual, rangeOfPredicate);

        AssertOwlNamedIndividual(objectIndividual);
        AssertOwlClass(objectIndividual, rangeOfPredicate);
        AssertProperty(subjectIndividual, customPredicate, objectIndividual);
    }

    private void AssertDatumValue(Graph ontologyGraph, INode subjectIndividual, INode predicate, string cellValue)
    {
        var datatypesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (datatypesOfPredicate.Count() != 1)
        {
            _logger.LogWarning($"Wrong number ({datatypesOfPredicate.Count()}) of datatypes for predicate {predicate}");
            AssertValueWithoutDatatype(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumValue()), cellValue);
        }

        var datatype = datatypesOfPredicate.First();
        AssertValueWithDatatype(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumValue()), datatype, cellValue);
    }

    private void AssertValue(Graph ontologyGraph, INode subjectIndividual, INode predicate, string cellValue)
    {
        var datatypesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (datatypesOfPredicate.Count() != 1)
        {
            _logger.LogWarning($"Wrong number ({datatypesOfPredicate.Count()}) of datatypes for predicate {predicate}");
            AssertValueWithoutDatatype(subjectIndividual, predicate, cellValue);
        }

        var datatype = datatypesOfPredicate.First();
        AssertValueWithDatatype(subjectIndividual, predicate, datatype, cellValue);
    }

    private void AssertValueWithDatatype(INode subjectIndividual, INode predicate, INode datatype, string cellValue)
    {
        var typedValueLiteral = _graph.CreateLiteralNode(cellValue.ToString(), new Uri(datatype.ToString()));
        AssertTriple(subjectIndividual, predicate, typedValueLiteral);
    }

    private void AssertValueWithoutDatatype(INode subjectIndividual, INode predicate, string cellValue)
    {
        AssertTriple(subjectIndividual, predicate, _graph.CreateLiteralNode(cellValue.ToString()));
    }

    private void AssertUomProperty(Graph ontologyGraph, INode subjectIndividual, INode predicate)
    {
        var uomsOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (uomsOfPredicate.Count() != 1)
        {
            _logger.LogWarning($"Wrong number of units of measure for predicate {predicate}");
            return;
        }

        var uom = uomsOfPredicate.First();
        AssertProperty(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumUOM()), uom);
    }

    private void AssertTriple(INode subjectIndividual, INode predicate, INode rangeIndividual)
    {
        _graph.Assert(subjectIndividual, predicate, rangeIndividual);
    }

    private bool IsNull(object value)
    {
        return value == null || value == DBNull.Value || value.ToString() == string.Empty;
    }

    private static int counter = 0;

    private Dictionary<string, string> GetOntologyNamespace(Graph graph)
    {
        var namespaces = graph.GetTriplesWithObject(graph.CreateUriNode(RdfCommonClasses.CreateNamespaceClass()));
        var prefixNamespace = namespaces.ToDictionary(ns => GetPrefix(graph, ns.Subject),
                                                        ns => ns.Subject.ToString());
        return prefixNamespace;
    }

    private string GetPrefix(Graph graph, INode subject)
    {
        var prefixTriple = graph.GetTriplesWithSubject(subject).First(triple => triple.Predicate.ToString() == RdfCommonProperties.CreateHasPrefix().AbsoluteUri);

        return prefixTriple != null ? prefixTriple.Object.ToString() : $"ns{counter++}";
    }
}