namespace Common.FusekiModels;

public class FusekiSelectResponse
{
    public FusekiHead Head { get; set; } = new();
    public FusekiResult Results { get; set; } = new();
}
