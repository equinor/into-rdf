using Common.ProvenanceModels;
using System.Data;

namespace Services.TransformationServices.RdfPreprocessingServices;

public interface IRdfPreprocessingService
{
    public DataSet CreateRdfTables(Provenance provenance, DataTable inputData);
}