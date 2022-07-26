using Common.SpreadsheetModels;
using System.Data;

namespace Services.DomReaderServices.MelDomReaderServices;

public interface IMelDomReaderService
{
    SpreadsheetInfo GetSpreadsheetInfo(Stream excelFile, string fileName);
    DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
}