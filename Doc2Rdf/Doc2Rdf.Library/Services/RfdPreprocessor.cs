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

    public DataSet CreateRdfTables(Provenance provenance, DataSet inputData)
    {   
        var dataCollectionUri = CreateDataCollectionUri(provenance);
        var transformationUri = CreateTransformationUri(provenance);

        var rdfDataSet = new DataSet();
        rdfDataSet.Tables.Add(CreateProvenanceTable(dataCollectionUri, provenance));
        rdfDataSet.Tables.Add(CreateTransformationTable(dataCollectionUri, provenance, transformationUri));

        for (int i = 0; i < inputData.Tables.Count; i++)
        {
            if (inputData.Tables[i].TableName.Contains("PhaseFilter"))
            {
                rdfDataSet.Tables.Add(CreatePhaseFilterTable(dataCollectionUri, provenance, transformationUri, inputData.Tables[i]));
                continue;
            }
            else if (inputData.Tables[i].TableName.Contains("PhaseCode"))
            {
                rdfDataSet.Tables.Add(CreatePhaseCodeTable(dataCollectionUri, provenance, transformationUri, inputData.Tables[i]));
                continue;
            }

            if (provenance.DataSource == DataSource.Mel)
            {
                rdfDataSet.Tables.Add(CreateDataCollectionTable(dataCollectionUri, provenance, inputData.Tables[i]));
            }
            rdfDataSet.Tables.Add(CreateInputTable(dataCollectionUri, provenance, transformationUri, inputData.Tables[i]));
        }

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
        RdfMelTableBuilder tableBuilder = (RdfMelTableBuilder)_rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);

        tableBuilder.AddTableName("DataCollection");
        tableBuilder.CreateDataCollectionSchema();
        tableBuilder.AddDataCollectionRows(dataCollectionUri, inputData);
        return tableBuilder.GetDataTable();
    }

    private DataTable CreatePhaseFilterTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri, DataTable phaseFilterTable)
    {
        RdfShipWeightTableBuilder tableBuilder = (RdfShipWeightTableBuilder)_rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        tableBuilder.AddTableName(phaseFilterTable.TableName);
        tableBuilder.CreatePhaseFilterSchema(provenance, phaseFilterTable.Columns);
        tableBuilder.AddPhaseFilterRows(dataCollectionUri, provenance, transformationUri, phaseFilterTable);
        return tableBuilder.GetDataTable();
    }

    private DataTable CreatePhaseCodeTable(Uri dataCollectionUri, Provenance provenance, Uri transformationUri, DataTable phaseCodeTable)
    {
        RdfShipWeightTableBuilder tableBuilder = (RdfShipWeightTableBuilder)_rdfTableBuilderFactory.GetRdfTableBuilder(provenance.DataSource);
        tableBuilder.AddTableName(phaseCodeTable.TableName);
        tableBuilder.CreatePhaseCodeSchema(provenance, phaseCodeTable.Columns);
        tableBuilder.AddPhaseCodeRows(dataCollectionUri, transformationUri, phaseCodeTable);
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
        var facilityIdentifier = (provenance.Facility.FacilityId + "/" + (provenance.DataSource == DataSource.Mel ?
                                    provenance.Facility.DocumentProjectId :
                                    provenance.Facility.SAPPlantId)).ToLower();

        var dataSource = provenance.DataSource.ToString().ToLower();

        var dataCollectionUri = new Uri($"{RdfPrefixes.Prefix2Uri["equinor"]}{facilityIdentifier}/{dataSource}/{provenance.RevisionNumber}");

        return dataCollectionUri;
    }

    private Uri CreateTransformationUri(Provenance provenance)
    {
        var dataSource = provenance.DataSource.ToString().ToLower();
        var transformationUri = new Uri($"{RdfPrefixes.Prefix2Uri["transformation"]}{dataSource}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");

        return transformationUri;
    }
}