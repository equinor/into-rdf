using VDS.RDF;

namespace Services.TransformationServices.RecordService;

public interface IRecordTransformationService
{
    public Graph CreateProtoRecord(Uri record, Graph graph);
}