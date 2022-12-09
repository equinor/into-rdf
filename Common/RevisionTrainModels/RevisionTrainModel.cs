namespace Common.RevisionTrainModels
{
    public class RevisionTrainModel
    {
        public RevisionTrainModel(Uri trainUri, string name, string tripleStore, string trainType)
        {
            TrainUri = trainUri;
            Name = name;
            TripleStore = tripleStore;
            TrainType = trainType;
            Records = new List<RecordModel>();
        }

        public Uri TrainUri { get; set; }
        public string Name { get; set; }
        public string TripleStore { get; set; }
        public string TrainType { get; set; }
        public TieContext? TieContext { get; set; }
        public SpreadsheetContext? SpreadsheetContext { get; set; }
        public List<RecordModel> Records {get; set; }
    }
}
