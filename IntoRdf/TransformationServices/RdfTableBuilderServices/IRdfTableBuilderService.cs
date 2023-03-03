using IntoRdf.Public.Models;
using System.Data;

namespace IntoRdf.TransformationServices.RdfTableBuilderServices;

internal interface IExcelRdfTableBuilderService
{
    DataTable GetInputDataTable(TransformationDetails transformationSettings, DataTable inputData);
}