using Common.ProvenanceModels;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace Doc2Rdf.Library.Services;

internal class RdfTransformer : IRdfTransformer
{
    private readonly IRdfPreprocessor _rdfPreprocessor;
    private readonly IRdfGraphWrapper _rdfGraphWrapper;
    private readonly ILogger<RdfTransformer> _logger;
    public RdfTransformer(IRdfPreprocessor rdfPreprocessor,
                          IRdfGraphWrapper rdfGraphWrapper,
                          ILogger<RdfTransformer> logger)
    {
        _rdfPreprocessor = rdfPreprocessor;
        _rdfGraphWrapper = rdfGraphWrapper;
        _logger = logger;
    }

    public string Transform(Provenance provenance, DataTable inputData)
    {
        var rdfDataSet = _rdfPreprocessor.CreateRdfTables(provenance, inputData);
        _logger.LogInformation("<RdfTransformer> - Transform: Dataset with {nbOfTables} tables created", rdfDataSet.Tables.Count);

        foreach (DataTable table in rdfDataSet.Tables)
        {
            _logger.LogDebug("<RdfTransformer> - Transform: Asserting data from table: {tableName}", table.TableName);
            _rdfGraphWrapper.AssertDataTable(table);
            _logger.LogDebug("<RdfTransformer> - Transform: Asserted data from table: {tableName}", table.TableName);
        }

        return _rdfGraphWrapper.WriteGraphToString();
    }
}
