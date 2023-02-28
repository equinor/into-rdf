using IntoRdf.Public.Models;
using VDS.RDF;

namespace IntoRdf.Services.TransformationServices.SpreadsheetServices;

internal interface ISpreadsheetService
{
    Graph ConvertToRdf(SpreadsheetTransformationDetails transformationDetails, Stream content);
}
