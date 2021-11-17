using System.IO;

namespace Excel2Turtle.Interfaces
{

    public interface ITtlMapper
    {
        string Map(string filename, Stream inputStream);
    }
}
