using IntoRdf.Public.Models;
using System.Data;

namespace IntoRdf.Services.DomReaderServices.ExcelDomReaderServices;

internal interface IExcelDomReaderService
{
    public DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details, string? identityColumn);
}