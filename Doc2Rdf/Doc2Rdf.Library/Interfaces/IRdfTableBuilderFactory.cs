using Doc2Rdf.Library.Interfaces;

namespace Doc2Rdf.Library.Interfaces;

public interface IRdfTableBuilderFactory
{
    IRdfTableBuilder GetRdfTableBuilder(string dataSource);
}
