using System.Data;
using Common.ProvenanceModels;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IRdfTransformer
    {
        public string Transform(Provenance provenance, DataTable inputData);
    }
}