using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;

namespace Doc2Rdf.Library
{
    public class RdfPreprocessor : IRdfPreprocessor
    {  
        RdfTableBuilderFactory _builderFactory;

        public RdfPreprocessor(DataSource dataSource)
        {
            _builderFactory = new RdfTableBuilderFactory(dataSource);
        }
        public DataSet CreateRdfTables(Provenance provenance, DataSet inputData)
        {
            var dataCollectionUri = CreateDataCollectionUri(provenance);
            var transformationUri = CreateTransformationUri(provenance);

            var rdfDataSet = new DataSet();
            rdfDataSet.Tables.Add(CreateProvenanceTable(dataCollectionUri, provenance));
            rdfDataSet.Tables.Add(CreateTransformationTable(dataCollectionUri, transformationUri));

            for (int i = 0; i < inputData.Tables.Count; i++)
            {
                if (inputData.Tables[i].TableName.Contains("PhaseFilter"))
                {
                    rdfDataSet.Tables.Add(CreatePhaseFilterTable(dataCollectionUri, transformationUri, inputData.Tables[i]));
                    continue;
                } 
                else if (inputData.Tables[i].TableName.Contains("PhaseCode"))
                {
                    rdfDataSet.Tables.Add(CreatePhaseCodeTable(dataCollectionUri, transformationUri, inputData.Tables[i]));
                    continue;
                }

                rdfDataSet.Tables.Add(CreateInputTable(dataCollectionUri, transformationUri, inputData.Tables[i]));
            }

            return rdfDataSet; 
        }

        private DataTable CreateProvenanceTable(Uri dataCollectionUri, Provenance provenance)
        {
            IRdfTableBuilder tableBuilder = _builderFactory.GetRdfTableBuilder("Provenance");
            tableBuilder.CreateProvenanceSchema();
            tableBuilder.AddProvenanceRow(dataCollectionUri, provenance);
            return tableBuilder.GetDataTable();
        }

        private DataTable CreateTransformationTable(Uri dataCollectionUri, Uri transformationUri)
        {
            IRdfTableBuilder tableBuilder = _builderFactory.GetRdfTableBuilder("Transformation");
            tableBuilder.CreateTransformationSchema();
            tableBuilder.AddTransformationRow(dataCollectionUri, transformationUri);
            return tableBuilder.GetDataTable();
        }

        private DataTable CreatePhaseFilterTable(Uri dataCollectionUri, Uri transformationUri, DataTable phaseFilterTable)
        {
            RdfShipWeightTableBuilder tableBuilder = (RdfShipWeightTableBuilder) _builderFactory.GetRdfTableBuilder(phaseFilterTable.TableName);
            tableBuilder.CreatePhaseFilterSchema(phaseFilterTable.Columns);
            tableBuilder.AddPhaseFilterRows(dataCollectionUri, transformationUri, phaseFilterTable);
            return tableBuilder.GetDataTable();
        }

        private DataTable CreatePhaseCodeTable(Uri dataCollectionUri, Uri transformationUri, DataTable phaseCodeTable)
        {
            RdfShipWeightTableBuilder tableBuilder = (RdfShipWeightTableBuilder) _builderFactory.GetRdfTableBuilder(phaseCodeTable.TableName);
            tableBuilder.CreatePhaseCodeSchema(phaseCodeTable.Columns);
            tableBuilder.AddPhaseCodeRows(dataCollectionUri, transformationUri, phaseCodeTable);
            return tableBuilder.GetDataTable();
        }

        private DataTable CreateInputTable(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
        {
            IRdfTableBuilder tableBuilder = _builderFactory.GetRdfTableBuilder(inputData.TableName);
            tableBuilder.CreateInputDataSchema(inputData.Columns);
            tableBuilder.AddInputDataRows(dataCollectionUri, transformationUri, inputData);
            return tableBuilder.GetDataTable();
        }

        private Uri CreateDataCollectionUri(Provenance provenance)
        {            
            var facilityIdentifier = provenance.DataSource == DataSource.Mel ? 
                                        provenance.Facility.DocumentProjectId :
                                        provenance.Facility.SAPPlantId;

            var dataSource = provenance.DataSource.ToString().ToLower();
            var dataCollectionUri = new Uri($"{Prefixes.Prefix2Uri["ext"]}{dataSource}/{facilityIdentifier}_{provenance.RevisionNumber}");

            return dataCollectionUri;
        }

        private Uri CreateTransformationUri(Provenance provenance)
        {
            var dataSource = provenance.DataSource.ToString().ToLower();
            var transformationUri = new Uri($"{Prefixes.Prefix2Uri["transformation"]}{dataSource}_{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd")}");

            return transformationUri;
        }
    }
}