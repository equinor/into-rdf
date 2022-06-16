
using Common.ProvenanceModels;
using System.Data;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IRdfPreprocessor
    {
        DataSet CreateRdfTables(Provenance provenance, DataTable inputData);
    }
}
