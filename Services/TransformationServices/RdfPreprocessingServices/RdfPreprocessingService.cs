using Common.ProvenanceModels;
using Common.RdfModels;
using Microsoft.Extensions.Logging;
using Services.TransformationServices.RdfTableBuilderServices;
using System.Data;

namespace Services.TransformationServices.RdfPreprocessingServices;

public class RdfPreprocessingService : IRdfPreprocessingService
{
    IRdfTableBuilderFactory _rdfTableBuilderFactory;
    ILogger<RdfPreprocessingService> _logger;

    public RdfPreprocessingService(IRdfTableBuilderFactory rdfTableBuilderFactory, ILogger<RdfPreprocessingService> logger)
    {
        _rdfTableBuilderFactory = rdfTableBuilderFactory;
        _logger = logger;
    }

    public DataSet CreateRdfTables(Provenance provenance, DataTable inputData)
    {
        if (provenance.DataSource == null)
        {
            throw new ArgumentException("Provenance does not contain datasource");
        }

        var dataCollectionUri = CreateDataCollectionUri(provenance);

        _logger.LogDebug("<RdfPreprocessor> - CreateRdfTables: Data collection uri {dataCollectionUri} created", dataCollectionUri.ToString());
        var transformationUri = CreateTransformationUri(provenance);
        _logger.LogDebug("<RdfPreprocessor> - CreateRdfTables: Transformation uri {transformationUri} created", transformationUri.ToString());

        var rdfDataSet = new DataSet();
        rdfDataSet.Tables.Add(CreateProvenanceTable(dataCollectionUri, provenance));
        _logger.LogDebug(rdfDataSet.Tables.Count == 1 ?
                        "<RdfPreprocessor> - CreateRdfTables: Provenance successfully added" :
                        "<RdfPreprocessor> - CreateRdfTables: Failed to create provenance");

        rdfDataSet.Tables.Add(CreateTransformationTable(dataCollectionUri, provenance, transformationUri));
        _logger.LogDebug(rdfDataSet.Tables.Count == 2 ?
                        "<RdfPreprocessor> - CreateRdfTables: Transformation successfully added" :
                        "<RdfPreprocessor> - CreateRdfTables: Failed to create transformation");

        //Temporary solution to ensure that current MEL transformation works until it can be migrated onto named graphs.
        if (provenance.DataSource != DataSource.LineList)
        {
            rdfDataSet.Tables.Add(CreateDataCollectionTable(dataCollectionUri, provenance, inputData));
            _logger.LogDebug(rdfDataSet.Tables.Count == 3 ?
                            "<RdfPreprocessor> - CreateRdfTables: Collection successfully added" :
                            "<RdfPreprocessor> - CreateRdfTables: Failed to create collection");
        }

        var dataTable = CreateInputTable(dataCollectionUri, provenance, transformationUri, inputData);
        rdfDataSet.Tables.Add(dataTable);

        _logger.LogDebug(dataTable != null ?
                        "<RdfPreprocessor> - CreateRdfTables: Input data successfully added" :
                        "<RdfPreprocessor> - CreateRdfTables: Failed to create input data");

        return rdfDataSet;
    }

    private DataTable CreateProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        IRdfTableBuilderService tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetProvenanceTable(dataCollectionUri, provenance);
    }

    private DataTable CreateTransformationTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri)
    {
        IRdfTableBuilderService tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetTransformationTable(dataCollectionUri, transformationUri);
    }

    private DataTable CreateDataCollectionTable(Uri dataCollectionUri, Provenance provenance, DataTable inputData)
    {
        IRdfTableBuilderService tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetDataCollectionTable(dataCollectionUri, inputData);
    }

    private DataTable CreateInputTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri, DataTable inputData)
    {
        IRdfTableBuilderService tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetInputDataTable(dataCollectionUri, transformationUri, provenance, inputData);
    }

    private Uri CreateDataCollectionUri(Provenance provenance)
    {
        var facilityIdentifierParts = new List<string> { provenance.FacilityId };
        if (!string.IsNullOrEmpty(provenance.DocumentProjectId))
            facilityIdentifierParts.Add(provenance.DocumentProjectId);
        else if (!string.IsNullOrEmpty(provenance.PlantId))
            facilityIdentifierParts.Add(provenance.PlantId);

        var facilityIdentifier = string.Join("/", facilityIdentifierParts).ToLower();

        //TODO - Temporary solution to keep the current mel-transformation while updating line list
        //Clean up in new flow
        var dataCollectionUri = new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}");

        if (provenance.DataSource is DataSource.LineList)
            dataCollectionUri = new Uri($"{dataCollectionUri}{provenance.FacilityId.ToLower()}/");
        else if (provenance.DataSource is DataSource.CommonLib)
            dataCollectionUri = new Uri(dataCollectionUri, $"{provenance.FacilityId.ToLower()}/{provenance.DataSourceTable}/");
        else
        {
            dataCollectionUri = provenance.DataSourceTable != null ?
                    new Uri($"{dataCollectionUri.AbsoluteUri}{facilityIdentifier}/{provenance.DataSource}/{provenance.DataSourceTable}/{provenance.RevisionName}") :
                    new Uri($"{dataCollectionUri.AbsoluteUri}{provenance.FacilityId.ToLower()}/{provenance.DocumentName}/{provenance.RevisionName}");
        }

        return dataCollectionUri;
    }

    private Uri CreateTransformationUri(Provenance provenance)
    {
        IRdfTableBuilderService tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder?.GetTransformationUri(provenance) 
            ?? new Uri($"{RdfPrefixes.Prefix2Uri["transformation"]}{provenance.DataSource}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");
    }
}