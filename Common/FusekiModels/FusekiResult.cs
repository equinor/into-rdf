namespace Common.FusekiModels;

public class FusekiResult
{
    public List<Dictionary<string, FusekiTriplet>> Bindings { get; set; } = new();
}
