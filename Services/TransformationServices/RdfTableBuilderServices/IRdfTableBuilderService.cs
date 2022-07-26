using System;
using System.Data;
using Common.ProvenanceModels;

namespace Services.TransformationServices.RdfTableBuilderServices;

public interface IRdfTableBuilderService
{
    DataTable GetProvenanceTable(Uri dataCollectionUri, Provenance provenance);
    DataTable GetTransformationTable(Uri dataCollectionUri, Uri transformationUri);
    DataTable GetDataCollectionTable(Uri dataCollectionUri, DataTable inputData);
    DataTable GetInputDataTable(Uri dataCollectionUri, Uri transformationUri, Provenance provenance, DataTable inputData);
    string GetBuilderType();
}