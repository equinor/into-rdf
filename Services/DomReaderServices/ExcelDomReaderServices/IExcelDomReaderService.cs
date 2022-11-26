using Common.RevisionTrainModels;
using Common.SpreadsheetModels;
using System.Data;

namespace Services.DomReaderServices.ExcelDomReaderServices;

public interface IExcelDomReaderService
{
    DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetContext spreadsheetContext);
}