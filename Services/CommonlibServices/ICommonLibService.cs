namespace Services.CommonlibServices;

public interface ICommonLibService
{
    Task<List<Dictionary<string, object>>> GetFromScopedLibrary(string library, string scope);
}