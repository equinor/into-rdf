using Microsoft.AspNetCore.Http;
using Common.GraphModels;
using Common.RevisionTrainModels;

namespace Services.RevisionServices;

public interface IRevisionTrainService
{
    Task<HttpResponseMessage> AddRevisionTrain(HttpRequest request);
    Task<HttpResponseMessage> RestoreRevisionTrain(string revisionTrain);
    Task<HttpResponseMessage> GetRevisionTrainByName(string name);
    Task<HttpResponseMessage> GetRevisionTrainByRecord(Uri record);
    Task<HttpResponseMessage> GetAllRevisionTrains();
    Task<HttpResponseMessage> DeleteRevisionTrain(string name);
    Task<HttpResponseMessage> AddRecordContext(ResultGraph context);
    ResultGraph CreateRecordContext(RevisionTrainModel train, string revisionName, DateTime revisionDate);
    Task<HttpResponseMessage> DeleteRecordContext(Uri record);    
}