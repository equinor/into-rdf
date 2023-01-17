using Common.RdfModels;
using Microsoft.Extensions.Logging;
using System.Data;
using VDS.RDF;
using VDS.RDF.Query.Builder;

namespace Services.TransformationServices.OntologyServices;

public class OntologyService : IOntologyService
{
    private Graph _graph;
    private ILogger<OntologyService> _logger;

    public OntologyService(ILogger<OntologyService> logger)
    {
        _graph = new Graph();
        _logger = logger;
    }

    private Graph InitializeGraph()
    {
        _graph = new Graph();
        foreach (var pair in RdfPrefixes.Prefix2Uri)
        {
            _graph.NamespaceMap.AddNamespace(pair.Key, pair.Value);
        }

        return _graph;
    }

    public Graph EnrichRdf(Graph ontologyGraph, Graph sourceGraph)
    {
        if (ontologyGraph.IsEmpty) { return sourceGraph; }

        InitializeGraph();
        var ontologyNamespaces = GetOntologyNamespace(ontologyGraph);

        foreach (var ns in ontologyNamespaces)
        {
            _graph.NamespaceMap.AddNamespace(ns.Key, new Uri(ns.Value));
        }

        var tripleCollection = sourceGraph.Triples;

        foreach (var triple in tripleCollection)
        {
            var superPredicates = GetSuperPredicates(ontologyGraph, triple.Predicate);

            if (superPredicates.Count() == 0)
            {
                AssertTriple(triple.Subject, triple.Predicate, triple.Object);
            }

            foreach (var superPredicate in superPredicates)
            {
                TraversePredicate(ontologyGraph, triple.Subject, triple.Subject, superPredicate, triple.Object);
            }
        }

        return GetGraph();
    }

    private Graph GetGraph()
    {
        return _graph;
    }

    private void TraversePredicate(Graph ontologyGraph, INode baseSubjectIndividual, INode subjectIndividual, INode predicate, INode objectIndividual)
    {
        if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateHasPhysicalQuantity()))
        {
            var rangesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);
            foreach (var rangeOfPredicate in rangesOfPredicate)
            {
                AssertRangesAsCustomProperty(baseSubjectIndividual, subjectIndividual, rangeOfPredicate, _graph.CreateUriNode(RdfCommonProperties.CreateHasPhysicalQuantity()));

                TraversePredicatesWithDomain(ontologyGraph, baseSubjectIndividual, rangeOfPredicate, objectIndividual);
            }
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateQuantityQualifiedAs()))
        {
            var rangesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);
            foreach (var rangeOfPredicate in rangesOfPredicate)
            {
                AssertRangesAsCustomProperty(baseSubjectIndividual, subjectIndividual, rangeOfPredicate, _graph.CreateUriNode(RdfCommonProperties.CreateQuantityQualifiedAs()));

                TraversePredicatesWithDomain(ontologyGraph, baseSubjectIndividual, rangeOfPredicate, objectIndividual);
            }
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatumUOM()))
        {
            AssertUomProperty(ontologyGraph, subjectIndividual, predicate);
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatumValue()))
        {
            AssertDatumValue(ontologyGraph, subjectIndividual, predicate, objectIndividual);
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateObjectProperty()))
        {
            //TODO - TO BE IMPLEMENTED WHEN KRAFLA LINE LIST IS COVERED BY ONTOLOGY
            AssertObjectIndividual(subjectIndividual, predicate, objectIndividual);
        }
        else if (HasSuperPredicate(ontologyGraph, predicate, RdfCommonProperties.CreateDatatypeProperty()))
        {
            AssertValue(ontologyGraph, subjectIndividual, predicate, objectIndividual);
        }
    }

    private IEnumerable<INode> GetRangesFromPredicate(Graph ontologyGraph, INode predicate)
    {
        var rangeTypes = ontologyGraph.GetTriplesWithSubject(predicate)
            .Where(t => t.Predicate.ToString() == RdfCommonProperties.CreateRange().ToString())
            .Select(t => t.Object);

        return rangeTypes;
    }

    private INode CreateIndividualWithTypeSuffix(INode individualBaseUri, INode individualType)
    {
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

    private void TraversePredicatesWithDomain(Graph ontologyGraph, INode baseSubjectIndividual, INode domainObject, INode objectIndividual)
    {
        var nextSubjectIndividual = CreateIndividualWithTypeSuffix(baseSubjectIndividual, domainObject);

        // The domainObject, is typically the range of a previously visited predicate, and the domain domain of one or several other predicates.
        // For instance:
        // :wallThicknessQuantifiedAs rdfs:range physical:WallThicknessDatum ; 
        // :wallThicknessDatumValue rdfs:domain physical:WallThicknessDatum ;
        // The transformation traverse the ontology by exploring the range/domain objects of predicates. 
        var predicatesWithDomain = GetPredicatesWithDomain(ontologyGraph, domainObject);

        foreach (var predicateWithDomain in predicatesWithDomain)
        {
            TraversePredicate(ontologyGraph, baseSubjectIndividual, nextSubjectIndividual, predicateWithDomain, objectIndividual);
        }
    }

    private string GetUriFragment(INode node)
    {
        return node.ToString().Split("#")[1];
    }

    private string GetUriPath(INode node)
    {
        var index = node.ToString().LastIndexOf("/");
        return node.ToString()[..index];
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

    private void AssertRangesAsCustomProperty(INode baseSubjectIndividual, INode subjectIndividual, INode rangeOfPredicate, INode customPredicate)
    {
        // New individuals are created as the ontology is traversed. 
        // The subject individual is the current individual                                         :  <https://rdf.equinor.com/wist/c277/20L00015A_WallThickness>
        // The rangeOfPredicate is a type given by the range relationship of a predicate.           :  <https://rdf.equinor.com/ontology/physical/v1#WallThicknessDatum>
        // The object individual is the individual created as a combination of the previous two     :  <https://rdf.equinor.com/wist/c277/20L00015A_WallThicknessDatum>
        var objectIndividual = CreateIndividualWithTypeSuffix(baseSubjectIndividual, rangeOfPredicate);

        AssertOwlNamedIndividual(objectIndividual);
        AssertOwlClass(objectIndividual, rangeOfPredicate);
        AssertProperty(subjectIndividual, customPredicate, objectIndividual);
    }

    private void AssertDatumValue(Graph ontologyGraph, INode subjectIndividual, INode predicate, INode objectIndividual)
    {
        var datatypesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (datatypesOfPredicate.Count() != 1)
        {
            _logger.LogWarning($"Wrong number ({datatypesOfPredicate.Count()}) of datatypes for predicate {predicate}");
            AssertValueWithoutDatatype(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumValue()), objectIndividual);
            return;
        }

        var datatype = datatypesOfPredicate.First();
        AssertValueWithDatatype(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumValue()), datatype, objectIndividual);
    }

    private void AssertValue(Graph ontologyGraph, INode subjectIndividual, INode predicate, INode objectIndividual)
    {
        var datatypesOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (datatypesOfPredicate.Count() != 1)
        {
            _logger.LogDebug($"Wrong number ({datatypesOfPredicate.Count()}) of datatypes for predicate {predicate}");
            AssertValueWithoutDatatype(subjectIndividual, predicate, objectIndividual);
            return;
        }

        var datatype = datatypesOfPredicate.First();
        AssertValueWithDatatype(subjectIndividual, predicate, datatype, objectIndividual);
    }

    private void AssertValueWithDatatype(INode subjectIndividual, INode predicate, INode datatype, INode objectIndividual)
    {
        var typedValueLiteral = _graph.CreateLiteralNode(objectIndividual.ToString(), new Uri(datatype.ToString()));
        AssertTriple(subjectIndividual, predicate, typedValueLiteral);
    }

    private void AssertValueWithoutDatatype(INode subjectIndividual, INode predicate, INode objectIndividual)
    {
        AssertTriple(subjectIndividual, predicate, objectIndividual);
    }

    private void AssertUomProperty(Graph ontologyGraph, INode subjectIndividual, INode predicate)
    {
        var uomsOfPredicate = GetRangesFromPredicate(ontologyGraph, predicate);

        if (uomsOfPredicate.Count() != 1)
        {
            _logger.LogDebug($"Wrong number of units of measure for predicate {predicate}");
            return;
        }

        var uom = uomsOfPredicate.First();
        AssertProperty(subjectIndividual, _graph.CreateUriNode(RdfCommonProperties.CreateDatumUOM()), uom);
    }

    private void AssertObjectIndividual(INode subjectIndividual, INode predicate, INode objectIndividual)
    {
        var path = GetUriPath(subjectIndividual);
        var objectUriNode = _graph.CreateUriNode(new Uri(path + objectIndividual.ToString()));
        AssertTriple(subjectIndividual, predicate, objectUriNode);
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