using System.IO;

namespace Doc2Rdf.Common.Interfaces
{

    public interface ITtlMapper
    {
        string Map(string filename, Stream inputStream);
    }
}
