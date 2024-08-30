using System.Data;
using VDS.RDF;

namespace IntoRdf.TransformationServices;

internal interface IRdfAssertionService
{
    public Graph AssertProcessedData(DataTable dataTable);

    public Graph AssertProcessedData(DataTable dataTable, string subjectColumnName);
}