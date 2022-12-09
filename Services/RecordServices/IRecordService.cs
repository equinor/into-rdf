using Common.RevisionTrainModels;

namespace Services.RecordServices;

public interface IRecordService
{
    string TransformExcel(RevisionTrainModel train, Stream content);
    Task<string> Add(RecordInputModel recordInput);
    Task<HttpResponseMessage> Delete(string server, Uri record);
    Task<HttpResponseMessage> Delete(string server, List<Uri> records);
}