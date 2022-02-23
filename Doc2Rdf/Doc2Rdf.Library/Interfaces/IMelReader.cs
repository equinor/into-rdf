using Doc2Rdf.Library.Models;
using System.Data;
using System.IO;

namespace Doc2Rdf.Library.Interfaces
{
    internal interface IMelReader
    {
        SpreadsheetDetails GetSpreadsheetDetails(FileStream excelFile);
        DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
    }
}
