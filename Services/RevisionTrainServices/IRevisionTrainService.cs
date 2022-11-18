using Microsoft.AspNetCore.Http;

namespace Services.RevisionTrainServices;

public interface IRevisionTrainService
{
    Task<HttpResponseMessage> CreateRevisionTrain(HttpRequest turtle);
    Task<HttpResponseMessage> GetRevisionTrain(string name);
    Task<HttpResponseMessage> GetAllRevisionTrains();
    Task<HttpResponseMessage> DeleteRevisionTrain(string name);
}