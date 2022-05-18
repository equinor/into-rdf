using Doc2Rdf.Library.Models;
using System.Data;
using System.IO;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IMelReader
    {
        SpreadsheetDetails GetSpreadsheetDetails(Stream excelFile, string fileName);
        DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
    }
}
