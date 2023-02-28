using System.Data;
using VDS.RDF;

namespace IntoRdf.Services.TransformationServices.OntologyServices;

internal interface IOntologyService
{
     Graph EnrichRdf(Graph sourceGraph, Graph ontologyGraph);
}