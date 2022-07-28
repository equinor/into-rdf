
namespace Common.ProvenanceModels;

public class RevisionRequirement
{
    public RevisionRequirement(string documentProjectId, string revisionName, DateTime revisionDate)
    {
        DocumentProjectId = documentProjectId;
        RevisionName = revisionName;
        RevisionDate = revisionDate;
    }

    public string DocumentProjectId { get; }
    public string RevisionName { get; }
    public DateTime RevisionDate { get; }
}