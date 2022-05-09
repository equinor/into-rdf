using Doc2Rdf.Library;

namespace Services.Doc2RdfService
{
    public class Doc2RdfService : IDoc2RdfService
    {
        public string PostDoc2Rdf(Stream stream, string fileName)
        {
            return MelTransformer.Transform(stream, fileName);
        }
    }
}
