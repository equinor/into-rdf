namespace Services.Doc2RdfService
{
    public interface IDoc2RdfService
    {
        string PostDoc2Rdf(Stream stream, string fileName);
    }
}