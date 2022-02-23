using System;
using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    internal interface IRdfTableBuilder
    {
        void CreateProvenanceSchema();
        void AddProvenanceRow(Uri dataCollectionUri, Provenance provenance);
        void CreateTransformationSchema();
        void AddTransformationRow(Uri dataCollectionUri, Uri transformationUri);
        void CreateInputDataSchema(DataColumnCollection columns);
        void AddInputDataRows(Uri dataCollectionUri, Uri transformationUri, DataTable inputData);
        DataTable GetDataTable();
    }
}