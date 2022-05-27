using System;
using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IRdfPreprocessor
    {
        DataSet CreateRdfTables(Provenance provenance, DataTable inputData);
    }
}
