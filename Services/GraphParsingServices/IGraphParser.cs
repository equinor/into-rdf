using Common.RevisionTrainModels;

namespace Services.GraphParserServices;

public interface IGraphParser
{
    RevisionTrainModel ParseRevisionTrain(string revisionTrain);
}