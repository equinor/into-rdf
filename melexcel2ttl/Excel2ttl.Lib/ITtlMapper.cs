using System.IO;

namespace Excel2ttl.Interfaces
{

    public interface ITtlMapper
    {
        string Map(string filename, Stream inputStream);
    }
}
