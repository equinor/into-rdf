using VDS.RDF;

namespace Services.TransformationServices.RecordService;

public interface IRecordService
{
    public Graph CreateProtoRecord(Uri record, Graph graph);
}