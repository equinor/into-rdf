using Common.RevisionTrainModels;

namespace Services.RevisionServices;

public interface IRevisionService
{
    void ValidateRevision(RevisionTrainModel revisionTrain, string revision, DateTime date);
}