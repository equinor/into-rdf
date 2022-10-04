
namespace Common.ProvenanceModels;

public class RevisionRequirement
{
    public RevisionRequirement(string facilityId, string documentName, string revisionName, DateTime revisionDate)
    {
        FacilityId = facilityId;
        DocumentName = documentName;
        RevisionName = revisionName;
        RevisionDate = revisionDate;
    }

    public string FacilityId { get; }
    public string DocumentName { get; }
    public string RevisionName { get; }
    public DateTime RevisionDate { get; }
}