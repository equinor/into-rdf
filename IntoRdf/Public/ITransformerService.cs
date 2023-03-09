using IntoRdf.Public.Models;

namespace IntoRdf
{
    public interface ITransformerService
    {
        public string TransformAml(AmlTransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);
        public string TransformSpreadsheet(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content, RdfFormat outputFormat);
        public string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails);
        public string InferFromOntology(string ontology, string graphString, RdfFormat outputFormat);
        public string CreateProtoRecord(Uri record, string graphString, RdfFormat outputFormat);
    }
}