namespace Common.GraphModels;
public class ResultGraph
{
    public ResultGraph(string name, string content, bool isDefault = false)
    {
        Name = name;
        Content = content;
        IsDefault = isDefault;
    }
    public string Name { get; set; }
    public string Content { get; set; }
    public bool IsDefault { get; set; }
}