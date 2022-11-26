using Common.RevisionTrainModels;
using Microsoft.AspNetCore.Http;
using VDS.RDF;

namespace Services.RevisionServices;

public interface IRevisionTrainService
{
    Task<HttpResponseMessage> CreateRevisionTrain(HttpRequest turtle);
    Task<HttpResponseMessage> GetRevisionTrain(string name);
    Task<HttpResponseMessage> GetAllRevisionTrains();
    Task<HttpResponseMessage> DeleteRevisionTrain(string name);
}