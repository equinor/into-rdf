using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using System.Data;

namespace Services.TransformationServices.RdfPreprocessingServices;

public interface IRdfPreprocessingService
{
    public DataSet CreateRdfTables(Provenance provenance, DataTable inputData);
    public DataSet CreateRdfTable(RevisionTrainModel revisionTrain, DataTable inputData);
}