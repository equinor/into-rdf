using System.Data;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Interfaces
{
    internal class RdfTransformer : IRdfTransformer
    {
        public string Transform(Provenance provenance, DataSet inputData)
        {
            var preprocessor = new RdfPreprocessor(provenance.DataSource);
            var rdfDataSet = preprocessor.CreateRdfTables(provenance, inputData);

            var graphWrapper = new RdfGraphWrapper();

                foreach (DataTable table in rdfDataSet.Tables)
                {
                    graphWrapper.AssertDataTable(table);
                }

                return graphWrapper.WriteGraphToString();
        }
    }
}