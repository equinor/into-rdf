using Common.GraphModels;
using Common.ProvenanceModels;
using Common.RevisionTrainModels;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfTransformationServices;

public interface IRdfTransformationService
{
    ResultGraph Transform(Provenance provenance, Graph ontologyGraph, DataTable inputData);

    Graph Transform(RevisionTrainModel revisionTrainModel, DataTable inputData);
}