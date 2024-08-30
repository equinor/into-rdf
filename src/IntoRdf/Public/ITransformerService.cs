using IntoRdf.Models;
using VDS.RDF;

namespace IntoRdf
{
    public interface ITransformerService
    {
        public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);
        public string TransformSpreadsheet(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content);
        public IGraph TransformSpreadsheetToGraph(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content);
        public string TransformCsv(CsvDetails csvDetails, TransformationDetails transformationDetails, Stream content);
    }
}