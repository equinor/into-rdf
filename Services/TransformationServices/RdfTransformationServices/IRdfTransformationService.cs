using Common.ProvenanceModels;
using System.Data;
using VDS.RDF;

namespace Services.TransformationServices.RdfTransformationServices;

public interface IRdfTransformationService
{
    public string Transform(Provenance provenance, Graph ontologyGraph, DataTable inputData);
}