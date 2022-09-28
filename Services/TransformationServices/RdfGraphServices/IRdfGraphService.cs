using Common.GraphModels;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfGraphServices;

public interface IRdfGraphService
{
    void AssertDataTable(DataTable dataTable, Graph ontologyGraph);
    ResultGraph GetResultGraph(string datasource);
}