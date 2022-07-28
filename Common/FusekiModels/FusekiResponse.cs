namespace Common.FusekiModels;

public class FusekiResponse
{
    public FusekiHead Head { get; set; } = new();
    public FusekiResult Results { get; set; } = new();
}
