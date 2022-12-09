namespace Common.RevisionTrainModels;

public class RecordInputModel
{
    public RecordInputModel(string trainName, string revisionName, string revisionDate, Stream content, string contentType)
    {
        RevisionTrainName = trainName;
        RevisionName = revisionName;
        RevisionDate = revisionDate;
        Content = content;
        ContentType = contentType;
    }
    public string RevisionTrainName { get; }
    public string RevisionName { get; }
    public string RevisionDate { get; }
    public Stream Content { get; }
    public string ContentType { get; }
}