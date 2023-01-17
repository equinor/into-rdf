using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfGraphServices;

public interface IRdfGraphService
{
    public void AssertDataTable(DataTable dataTable);
    public Graph GetGraph();
}