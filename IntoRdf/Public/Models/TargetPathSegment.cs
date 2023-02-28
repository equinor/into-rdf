namespace IntoRdf.Public.Models;

public class TargetPathSegment
{
    public string Target { get; }
    public string Segment { get; }
    public bool IsIdentity { get; }

    public TargetPathSegment(string target, string segment, bool isIdentity = false)
    {
        Target = target;
        Segment = segment;
        IsIdentity = isIdentity;
    }
}
