using Common.GraphModels;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using Microsoft.Extensions.Logging;
using Services.TransformationServices.RdfGraphServices;
using Services.TransformationServices.RdfPreprocessingServices;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfTransformationServices;

public class RdfTransformationService : IRdfTransformationService
{
    private readonly IRdfPreprocessingService _rdfPreprocessor;
    private readonly IRdfGraphService _rdfGraphService;
    private readonly ILogger<RdfTransformationService> _logger;
    public RdfTransformationService(IRdfPreprocessingService rdfPreprocessor,
                          IRdfGraphService rdfGraphService,
                          ILogger<RdfTransformationService> logger)
    {
        _rdfPreprocessor = rdfPreprocessor;
        _rdfGraphService = rdfGraphService;
        _logger = logger;
    }

    public ResultGraph Transform(Provenance provenance, Graph ontologyGraph, DataTable inputData)
    {
        var rdfDataSet = _rdfPreprocessor.CreateRdfTables(provenance, inputData);
        _logger.LogInformation("<RdfTransformer> - Transform: Dataset with {nbOfTables} tables created", rdfDataSet.Tables.Count);

        foreach (DataTable table in rdfDataSet.Tables)
        {
            _logger.LogDebug("<RdfTransformer> - Transform: Asserting data from table: {tableName}", table.TableName);
            _rdfGraphService.AssertDataTable(table, ontologyGraph);
            _logger.LogDebug("<RdfTransformer> - Transform: Asserted data from table: {tableName}", table.TableName);
        }

        return _rdfGraphService.GetResultGraph(provenance.DataSource);
    }

    public string Transform(RevisionTrainModel revisionTrain, DataTable data)
    {
        var rdfDataSet = _rdfPreprocessor.CreateRdfTable(revisionTrain, data);

        foreach (DataTable table in rdfDataSet.Tables)
        {
            _logger.LogDebug("<RdfTransformer> - Transform: Asserting data from table: {tableName}", table.TableName);
            _rdfGraphService.AssertDataTable(table);
            _logger.LogDebug("<RdfTransformer> - Transform: Asserted data from table: {tableName}", table.TableName);
        }

        return _rdfGraphService.WriteGraphToString();
    }
}