public interface IFusekiAskService
{
    Task<bool> Ask(string server, string query);
}