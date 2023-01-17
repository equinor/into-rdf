using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.OntologyServices;

public interface IOntologyService
{
     Graph EnrichRdf(Graph sourceGraph, Graph ontologyGraph);
}