using Common.ProvenanceModels;

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
        var source =  (dataSource == DataSource.Mel || dataSource == DataSource.LineList) ? DataSource.Spreadsheet : dataSource;
        var builder = _rdfTableBuilderServices.FirstOrDefault(x => x.GetBuilderType() == source) ?? throw new ArgumentException($"Builder of type {source} not available");

        return builder;
    }
}
