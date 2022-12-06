using Common.RevisionTrainModels;
using Common.GraphModels;

namespace Services.RecordServices;

public interface IRecordService
{
    string TransformExcel(RevisionTrainModel train, Stream content);
    Task<HttpResponseMessage> Add(string server, ResultGraph record);
    Task<HttpResponseMessage> Delete(string server, Uri record);
    Task<HttpResponseMessage> Delete(string server, List<Uri> records);
}