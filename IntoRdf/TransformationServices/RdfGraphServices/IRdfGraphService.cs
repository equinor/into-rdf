using System.Data;
using VDS.RDF;

namespace IntoRdf.TransformationServices.RdfGraphServices;

internal interface IRdfGraphService
{
    internal void AssertDataTable(DataTable dataTable);
    internal Graph GetGraph();
}