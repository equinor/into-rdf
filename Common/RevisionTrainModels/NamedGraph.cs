namespace Common.RevisionTrainModels
{
    public class NamedGraph 
    {
        public NamedGraph(Uri namedGraphUri, string revisionName, DateTime revisionDate)
        {
            NamedGraphUri = namedGraphUri;
            RevisionName = revisionName;
            RevisionDate = revisionDate;
        }
        public Uri NamedGraphUri {get; set; }
        public string RevisionName { get; set; }
        public DateTime RevisionDate { get; set; }
        public int RevisionNumber {get; set; }
        public Uri? Replaces { get; set; }
    }
}
