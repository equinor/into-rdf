using IntoRdf.Public.Models;

namespace IntoRdf
{
    public interface ITransformerService
    {
        public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);
        public string TransformSpreadsheet(SpreadsheetTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);

        public string EnrichRdf(string ontology, string graphString, RdfFormat outputFormat);

        public string CreateProtoRecord(Uri record, string graphString, RdfFormat outputFormat);

    }
}