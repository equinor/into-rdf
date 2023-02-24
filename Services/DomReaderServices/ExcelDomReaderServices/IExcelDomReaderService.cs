using IntoRdf.TransformationModels;
using System.Data;

namespace Services.DomReaderServices.ExcelDomReaderServices;

public interface IExcelDomReaderService
{
    DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details, string? identityColumn);
}