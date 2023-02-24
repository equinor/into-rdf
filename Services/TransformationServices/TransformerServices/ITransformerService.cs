using IntoRdf.TransformationModels;

namespace Services.TransformerServices
{
    public interface ITransformerService
    {
        public string TransformSpreadsheet(SpreadsheetTransformationDetails transformationDetails, Stream content);

        public string EnrichRdf(string ontology, string graphString);

        public string CreateProtoRecord(Uri record, string graphString);

    }
}