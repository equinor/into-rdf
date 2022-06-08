namespace Common.ProvenanceModels;

public class RevisionInfo
{
    public Uri? FullName { get; set; }
    public int RevisionNumber { get; set; }
    public string? RevisionName { get; set; }
    public DateTime? RevisionDate { get; set; }
}