using System;
using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    internal interface IRdfPreprocessor
    {
        DataSet CreateRdfTables(Provenance provenance, DataSet inputData);
    }
}
