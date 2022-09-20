using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.SourceToOntologyConversionService;

public interface ISourceToOntologyConversionService
{
     void ConvertSourceToOntology(DataTable data, Graph ontologyGraph);

     Graph GetGraph();
}