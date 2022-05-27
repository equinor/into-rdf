using System.Data;
using Doc2Rdf.Library.Models;
using Doc2Rdf.Library.Interfaces;
using System;

namespace Doc2Rdf.Library.Services;

internal class RdfTransformer : IRdfTransformer
{
    IRdfPreprocessor _rdfPreprocessor;
    IRdfGraphWrapper _rdfGraphWrapper;
    public RdfTransformer(IRdfPreprocessor rdfPreprocessor, IRdfGraphWrapper rdfGraphWrapper)
    {
        _rdfPreprocessor = rdfPreprocessor;
        _rdfGraphWrapper = rdfGraphWrapper;
    }

    public string Transform(Provenance provenance, DataTable inputData)
    {
        var rdfDataSet = _rdfPreprocessor.CreateRdfTables(provenance, inputData);

        foreach (DataTable table in rdfDataSet.Tables)
        {
            _rdfGraphWrapper.AssertDataTable(table);
        }

        return _rdfGraphWrapper.WriteGraphToString();
    }
}
