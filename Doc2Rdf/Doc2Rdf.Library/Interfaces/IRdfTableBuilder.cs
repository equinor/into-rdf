using System;
using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IRdfTableBuilder
    {
        void AddTableName(string tableName);
        void CreateProvenanceSchema();
        void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance);
        void CreateTransformationSchema();
        void AddTransformationRow(Uri dataCollectionUri, Uri transformationUri);
        void CreateDataCollectionSchema();
        void AddDataCollectionRows(Uri dataCollectionUri, DataTable inputData);
        void CreateInputDataSchema(Provenance provenance, DataColumnCollection columns);
        void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData);
        DataTable GetDataTable();
    }
}