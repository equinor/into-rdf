using VDS.RDF;

namespace IntoRdf.Services.TransformationServices.RecordService;

internal interface IRecordTransformationService
{
    internal Graph CreateProtoRecord(Uri record, Graph graph);
}