using Common.Exceptions;
using Common.RevisionTrainModels;
using VDS.RDF;

namespace Services.RevisionServices;

public class RevisionService : IRevisionService
{
    public void ValidateRevision(RevisionTrainModel revisionTrain, string revision, DateTime date)
    {
        var existingRevisions = revisionTrain.Records;
        if (existingRevisions.Count() > 0)
        {
            var exists = existingRevisions.Any(ng => ng.RevisionName == revision);
            if (exists == true) { throw new RevisionValidationException($"A revision with name {revision} exists already for train {revisionTrain.Name}"); }

            var latestRevision = revisionTrain.Records.MaxBy(ng => ng.RevisionNumber);
            if (latestRevision == null) { throw new InvalidOperationException($"Failed to retrieve latest revision for revision train {revisionTrain.Name}"); }

            if (latestRevision.RevisionDate >= date) {throw new InvalidOperationException($"The new revision {revision} is older than the latest revision {latestRevision.RevisionName} for revision train {revisionTrain.Name}"); }
        }
    }
}