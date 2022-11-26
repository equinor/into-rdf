using Common.RevisionTrainModels;
using Common.GraphModels;

namespace Services.RecordServices;

public interface IRecordService
{
    Task<HttpResponseMessage> UploadExcel(RevisionTrainModel train, ResultGraph recordContext, Stream content);

    ResultGraph CreateRecordContext(RevisionTrainModel train, string revisionName, DateTime revisionDate);
}