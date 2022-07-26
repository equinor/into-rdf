using System.Data;

namespace Services.TransformationServices.RdfGraphServices;

public interface IRdfGraphService
{
    void AssertDataTable(DataTable dataTable);
    string WriteGraphToString();
}