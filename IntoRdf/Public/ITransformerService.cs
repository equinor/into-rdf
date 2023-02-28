using IntoRdf.Public.Models;

namespace IntoRdf
{
    public interface ITransformerService
    {
        public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);
        public string TransformSpreadsheet(SpreadsheetTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);

        internal string EnrichRdf(string ontology, string graphString, RdfFormat outputFormat);

        internal string CreateProtoRecord(Uri record, string graphString, RdfFormat outputFormat);

    }
}