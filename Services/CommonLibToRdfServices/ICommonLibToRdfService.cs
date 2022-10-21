using Common.GraphModels;

namespace Services.CommonLibToRdfServices;

public interface ICommonLibToRdfService
{
    Task<ResultGraph?> MoveCommonlibLibraryToRdf(string server, string library, string scope);
}