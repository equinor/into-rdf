using System.Data;

namespace Doc2Rdf.Library.Interfaces;

public interface IRdfGraphWrapper
{
    void AssertDataTable(DataTable dataTable);
    string WriteGraphToString();
}
