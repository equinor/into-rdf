using Common.RevisionTrainModels;
using VDS.RDF;

namespace Services.RecordServices;
public interface IRecordService
{
    Graph TransformExcel(RevisionTrainModel train, Stream content);
    Task<string> Add(RecordInputModel recordInput);
    Task<HttpResponseMessage> Delete(string server, Uri record);
    Task<HttpResponseMessage> Delete(string server, List<Uri> records);
}