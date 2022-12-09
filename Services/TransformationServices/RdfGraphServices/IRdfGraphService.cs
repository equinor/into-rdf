using Common.GraphModels;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfGraphServices;

public interface IRdfGraphService
{
    void AssertDataTable(DataTable dataTable);
    ResultGraph GetResultGraph(string datasource);
    string WriteGraphToString();
    Graph GetGraph();
}