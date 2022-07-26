using System.Data;
using Common.ProvenanceModels;

namespace Services.TransformationServices.RdfTransformationServices;

public interface IRdfTransformationService
{
    public string Transform(Provenance provenance, DataTable inputData);
}