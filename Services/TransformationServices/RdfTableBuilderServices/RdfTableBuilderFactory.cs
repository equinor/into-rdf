
namespace Services.TransformationServices.RdfTableBuilderServices;

public class RdfTableBuilderFactory : IRdfTableBuilderFactory
{
    private readonly IEnumerable<IRdfTableBuilderService> _rdfTableBuilderServices;

    public RdfTableBuilderFactory(IEnumerable<IRdfTableBuilderService> rdfTableBuilders)
    {
        _rdfTableBuilderServices = rdfTableBuilders;
    }

    public IRdfTableBuilderService GetRdfTableBuilder(string dataSource)
    {
        var builder = _rdfTableBuilderServices.FirstOrDefault(x => x.GetBuilderType() == dataSource) ?? throw new ArgumentException($"Builder of type {dataSource} not available");

        return builder;
    }
}
