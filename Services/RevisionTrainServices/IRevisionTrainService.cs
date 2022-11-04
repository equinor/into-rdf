using Microsoft.AspNetCore.Http;
using Common.RevisionTrainModels;

namespace Services.RevisionTrainServices;

public interface IRevisionTrainService
{
    Task<IResult> CreateRevisionTrain(HttpRequest turtle);
    Task<IResult> GetRevisionTrain(string name);
    Task<IResult> GetAllRevisionTrains();
    Task<IResult> DeleteRevisionTrain(string name);
}