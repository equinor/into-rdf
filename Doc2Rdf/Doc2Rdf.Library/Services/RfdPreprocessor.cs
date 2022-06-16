using Common.ProvenanceModels;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace Doc2Rdf.Library.Services;
public class RdfPreprocessor : IRdfPreprocessor
{
    IRdfTableBuilderFactory _rdfTableBuilderFactory;
    ILogger<RdfPreprocessor> _logger;

    public RdfPreprocessor(IRdfTableBuilderFactory rdfTableBuilderFactory, ILogger<RdfPreprocessor> logger)
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

        rdfDataSet.Tables.Add(CreateDataCollectionTable(dataCollectionUri, provenance, inputData));
        _logger.LogDebug(rdfDataSet.Tables.Count == 3 ? 
                        "<RdfPreprocessor> - CreateRdfTables: Collection successfully added" : 
                        "<RdfPreprocessor> - CreateRdfTables: Failed to create collection");

        rdfDataSet.Tables.Add(CreateInputTable(dataCollectionUri, provenance, transformationUri, inputData));
        _logger.LogDebug(rdfDataSet.Tables.Count == 4 ? 
                        "<RdfPreprocessor> - CreateRdfTables: Input data successfully added" : 
                        "<RdfPreprocessor> - CreateRdfTables: Failed to create input data");

        return rdfDataSet;
    }

    private DataTable CreateProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetProvenanceTable(dataCollectionUri, provenance);
    }

    private DataTable CreateTransformationTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetTransformationTable(dataCollectionUri, transformationUri);
    }

    private DataTable CreateDataCollectionTable(Uri dataCollectionUri, Provenance provenance, DataTable inputData)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetDataCollectionTable(dataCollectionUri, inputData);
    }

    private DataTable CreateInputTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri, DataTable inputData)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        return tableBuilder.GetInputDataTable(dataCollectionUri, transformationUri, provenance, inputData);
    }

    private Uri CreateDataCollectionUri(Provenance provenance)
    {
        var facilityIdentifier = (provenance.FacilityId + "/" + (provenance.DocumentProjectId != "na" ?
                                    provenance.DocumentProjectId :
                                    provenance.PlantId)).ToLower();

        var dataCollectionUri = provenance.DataSourceTable != null ?
                    new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}{facilityIdentifier}/{provenance.DataSource}/{provenance.DataSourceTable}/{provenance.RevisionName}") :
                    new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}{facilityIdentifier}/{provenance.DataSource}/{provenance.RevisionName}");
        return dataCollectionUri;
    }

    private Uri CreateTransformationUri(Provenance provenance)
    {
        var transformationUri = new Uri($"{RdfPrefixes.Prefix2Uri["transformation"]}{provenance.DataSource}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");

        return transformationUri;
    }
}