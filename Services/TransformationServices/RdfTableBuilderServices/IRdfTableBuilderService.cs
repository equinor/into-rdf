using System;
using System.Data;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;

namespace Services.TransformationServices.RdfTableBuilderServices;

public interface IRdfTableBuilderService
{
    DataTable GetProvenanceTable(Uri dataCollectionUri, Provenance provenance);
    DataTable GetTransformationTable(Uri dataCollectionUri, Uri transformationUri);
    DataTable GetDataCollectionTable(Uri dataCollectionUri, DataTable inputData);
    DataTable GetInputDataTable(Uri dataCollectionUri, RevisionTrainModel revisionTrainModel, DataTable inputData);
    DataTable GetInputDataTable(Uri dataCollectionUri, Uri transformationUri, Provenance provenance, DataTable inputData);
    string GetBuilderType();
    /// <summary>
    /// Overrides creation of transformation uri. When this returns null, we use the default way.
    /// </summary>
    Uri? GetTransformationUri(Provenance provenance);
}