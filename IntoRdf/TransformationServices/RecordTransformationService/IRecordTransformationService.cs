using VDS.RDF;

namespace IntoRdf.TransformationServices.RecordService;

internal interface IRecordTransformationService
{
    internal Graph CreateProtoRecord(Uri record, Graph graph);
}