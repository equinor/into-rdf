using Common.Constants;

namespace Common.GraphModels;
public class ResultGraph
{
    public ResultGraph(string name, string content)
    {
        Name = name;
        Content = content;
    }
    public string Name { get; set; }   
    public string Content { get; set; }
}