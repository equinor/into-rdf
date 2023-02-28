using IntoRdf.Public.Models;
using System.Data;

namespace IntoRdf.Services.TransformationServices.RdfTableBuilderServices;

internal interface IExcelRdfTableBuilderService
{
    DataTable GetInputDataTable(Uri dataCollectionUri, SpreadsheetTransformationDetails transformationSettings, DataTable inputData);
}