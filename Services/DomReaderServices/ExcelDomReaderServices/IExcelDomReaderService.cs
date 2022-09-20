using Common.SpreadsheetModels;
using System.Data;

namespace Services.DomReaderServices.ExcelDomReaderServices;

public interface IExcelDomReaderService
{
    SpreadsheetInfo GetSpreadsheetInfo(Stream excelFile, string fileName);
    DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
}