namespace Common.RevisionTrainModels
{
    public class RecordModels 
    {
        public RecordModels(Uri recordUri, string revisionName, DateTime revisionDate)
        {
            RecordUri = recordUri;
            RevisionName = revisionName;
            RevisionDate = revisionDate;
        }
        public Uri RecordUri {get; set; }
        public string RevisionName { get; set; }
        public DateTime RevisionDate { get; set; }
        public int RevisionNumber {get; set; }
        public Uri? Replaces { get; set; }
    }
}
