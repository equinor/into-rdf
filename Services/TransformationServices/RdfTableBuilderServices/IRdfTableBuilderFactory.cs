

namespace Services.TransformationServices.RdfTableBuilderServices;

public interface IRdfTableBuilderFactory
{
    IRdfTableBuilderService GetRdfTableBuilder(string dataSource);
}

