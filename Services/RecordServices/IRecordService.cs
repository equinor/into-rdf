using Common.RevisionTrainModels;

namespace Services.RecordServices;
public interface IRecordService
{
    Task<string> Transform(string revisionTrainName, Stream content, string contentType);
    Task<string> Add(RecordInputModel recordInput);
    Task Delete(Uri record);
}