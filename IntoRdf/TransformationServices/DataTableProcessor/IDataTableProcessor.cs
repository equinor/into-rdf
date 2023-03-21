using IntoRdf.Public.Models;
using System.Data;

namespace IntoRdf.TransformationServices.RdfTableBuilderServices;

internal interface IDataTableProcessor
{
    DataTable ProcessDataTable(TransformationDetails transformationSettings, DataTable inputData);
}