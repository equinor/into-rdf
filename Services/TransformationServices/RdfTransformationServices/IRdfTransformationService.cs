using Common.GraphModels;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfTransformationServices;

public interface IRdfTransformationService
{
    public ResultGraph Transform(Provenance provenance, Graph ontologyGraph, DataTable inputData);

    public string Transform(RevisionTrainModel revisionTrainModel, DataTable inputData);
}