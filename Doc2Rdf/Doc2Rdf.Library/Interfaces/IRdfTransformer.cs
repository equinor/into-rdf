using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IRdfTransformer
    {
        public string Transform(Provenance provenance, DataSet inputData);
    }
}