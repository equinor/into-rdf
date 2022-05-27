using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;

namespace Doc2Rdf.Library.Services;
public class RdfPreprocessor : IRdfPreprocessor
{
    IRdfTableBuilderFactory _rdfTableBuilderFactory;

    public RdfPreprocessor(IRdfTableBuilderFactory rdfTableBuilderFactory)
    {
        _rdfTableBuilderFactory = rdfTableBuilderFactory;
    }

    public DataSet CreateRdfTables(Provenance provenance, DataTable inputData)
    {   
        var dataCollectionUri = CreateDataCollectionUri(provenance);
        var transformationUri = CreateTransformationUri(provenance);

        var rdfDataSet = new DataSet();
        rdfDataSet.Tables.Add(CreateProvenanceTable(dataCollectionUri, provenance));
        rdfDataSet.Tables.Add(CreateTransformationTable(dataCollectionUri, provenance, transformationUri));

        rdfDataSet.Tables.Add(CreateDataCollectionTable(dataCollectionUri, provenance, inputData));
        rdfDataSet.Tables.Add(CreateInputTable(dataCollectionUri, provenance, transformationUri, inputData));
    
        return rdfDataSet;
    }

    private DataTable CreateProvenanceTable(Uri dataCollectionUri, Provenance provenance)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        tableBuilder.AddTableName("Provenance");
        tableBuilder.CreateProvenanceSchema();
        tableBuilder.AddProvenanceRow(dataCollectionUri, provenance);
        return tableBuilder.GetDataTable();
    }

    private DataTable CreateTransformationTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        
        tableBuilder.AddTableName("Transformation");
        tableBuilder.CreateTransformationSchema();
        tableBuilder.AddTransformationRow(dataCollectionUri, transformationUri);
        return tableBuilder.GetDataTable();
    }

    private DataTable CreateDataCollectionTable(Uri dataCollectionUri, Provenance provenance, DataTable inputData)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);

        tableBuilder.AddTableName("DataCollection");
        tableBuilder.CreateDataCollectionSchema();
        tableBuilder.AddDataCollectionRows(dataCollectionUri, inputData);
        return tableBuilder.GetDataTable();
    }

    private DataTable CreateInputTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri, DataTable inputData)
    {
        IRdfTableBuilder tableBuilder = _rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        tableBuilder.AddTableName(inputData.TableName);
        tableBuilder.CreateInputDataSchema(provenance, inputData.Columns);
        tableBuilder.AddInputDataRows(dataCollectionUri, transformationUri, inputData);
        return tableBuilder.GetDataTable();
    }

    private Uri CreateDataCollectionUri(Provenance provenance)
    {
        
        var facilityIdentifier = (provenance.Facility.FacilityId + "/" + (provenance.Facility.DocumentProjectId != "NA" ?
                                    provenance.Facility.DocumentProjectId :
                                    provenance.Facility.SAPPlantId)).ToLower();

        var dataCollectionUri = provenance.TableName != "na" ?
                    new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}{facilityIdentifier}/{provenance.DataSource}/{provenance.TableName}/{provenance.RevisionNumber}") :
                    new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}{facilityIdentifier}/{provenance.DataSource}/{provenance.RevisionNumber}");
        return dataCollectionUri;
    }

    private Uri CreateTransformationUri(Provenance provenance)
    {
        var transformationUri = new Uri($"{RdfPrefixes.Prefix2Uri["transformation"]}{provenance.DataSource}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");

        return transformationUri;
    }
}