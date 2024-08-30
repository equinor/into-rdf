using IntoRdf.Models;
using VDS.RDF;

namespace IntoRdf.TransformationServices;

internal interface ICsvService
{
    Graph ConvertToRdf(CsvDetails csvDetails, TransformationDetails transformationDetails, Stream content);
}
