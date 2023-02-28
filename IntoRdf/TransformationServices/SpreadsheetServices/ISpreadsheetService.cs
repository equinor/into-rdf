using IntoRdf.Public.Models;
using VDS.RDF;

namespace IntoRdf.TransformationServices.SpreadsheetServices;

internal interface ISpreadsheetService
{
    Graph ConvertToRdf(SpreadsheetTransformationDetails transformationDetails, Stream content);
}
