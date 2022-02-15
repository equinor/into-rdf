using Doc2Rdf.Library.Models;
using System.IO;

namespace Doc2Rdf.Library
{
    internal interface IMelReader
    {
        SpreadsheetDetails GetSpreadsheetDetails(FileStream excelFile);
        ExcelData GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
    }
}
