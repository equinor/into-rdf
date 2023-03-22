using IntoRdf.Public.Models;
using System.Data;

namespace IntoRdf.TransformationServices;

internal interface IDataTableProcessor
{
    DataTable ProcessDataTable(TransformationDetails transformationSettings, DataTable inputData);
}