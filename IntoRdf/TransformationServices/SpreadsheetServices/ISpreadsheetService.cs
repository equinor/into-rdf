using IntoRdf.Public.Models;
using VDS.RDF;

namespace IntoRdf.TransformationServices.SpreadsheetServices;

internal interface ISpreadsheetService
{
    Graph ConvertToRdf(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content);
}
