using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.SourceToOntologyConversionService;

public interface ISourceToOntologyConversionService
{
     Graph ConvertSourceToOntology(Graph sourceGraph, Graph ontologyGraph);
}