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
        public DataSet CreateRdfTables(Provenance provenance, DataTable inputData)
        {
            var dataCollectionUri = CreateDataCollectionUri(provenance);
            var transformationUri = CreateTransformationUri(provenance);

            var rdfDataSet = new DataSet();
            rdfDataSet.Tables.Add(CreateProvenanceTable(dataCollectionUri, provenance));
            rdfDataSet.Tables.Add(CreateTransformationTable(dataCollectionUri, transformationUri));
            rdfDataSet.Tables.Add(CreateInputTable(dataCollectionUri, transformationUri, inputData));

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

        private DataTable CreateInputTable(Uri dataCollectionUri, Uri transformationUri, DataTable inputData)
        {
            IRdfTableBuilder tableCreator = _builderFactory.GetRdfTableBuilder("InputData");
            tableCreator.CreateInputDataSchema(inputData.Columns);
            tableCreator.AddInputDataRows(dataCollectionUri, transformationUri, inputData);
            return tableCreator.GetDataTable();
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