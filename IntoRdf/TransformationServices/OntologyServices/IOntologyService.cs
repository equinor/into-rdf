using VDS.RDF;

namespace IntoRdf.TransformationServices.OntologyServices;

internal interface IOntologyService
{
     Graph EnrichRdf(Graph sourceGraph, Graph ontologyGraph);
}