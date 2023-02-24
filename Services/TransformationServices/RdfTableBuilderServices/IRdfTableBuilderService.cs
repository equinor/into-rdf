using System.Data;
using IntoRdf.TransformationModels;

namespace Services.TransformationServices.RdfTableBuilderServices;

public interface IExcelRdfTableBuilderService
{
    DataTable GetInputDataTable(Uri dataCollectionUri, SpreadsheetTransformationDetails transformationSettings, DataTable inputData);
}