using IntoRdf.TransformationModels;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetServices;

public interface ISpreadsheetService
{
    Graph ConvertToRdf(SpreadsheetTransformationDetails transformationDetails, Stream content);
}
