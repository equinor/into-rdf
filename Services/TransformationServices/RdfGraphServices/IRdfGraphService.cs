using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfGraphServices;

public interface IRdfGraphService
{
    void AssertDataTable(DataTable dataTable, Graph ontologyGraph);
    string WriteGraphToString();
}