namespace Common.TransformationModels;

public class SpreadsheetTransformationDetails
{
    public SpreadsheetDetails SpreadsheetDetails { get; set; }
    public Uri Record { get; set; }
    public List<string> IriSegments { get; set; }
    public string? TransformationType { get; set; }
    public EnrichmentLevel Level {get; set;}
    public List<TargetPathSegment> TargetPathSegments { get; set; }

    public SpreadsheetTransformationDetails(Uri record, SpreadsheetDetails spreadsheetDetails)
    {
        SpreadsheetDetails = spreadsheetDetails;
        Record = record;
        IriSegments = new List<string>();
        Level = EnrichmentLevel.None;
        TargetPathSegments = new List<TargetPathSegment>();
    }
}