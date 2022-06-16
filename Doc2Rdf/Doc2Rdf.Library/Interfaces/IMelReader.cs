using Common.SpreadsheetModels;
using System.Data;
using System.IO;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IMelReader
    {
        SpreadsheetInfo GetSpreadsheetInfo(Stream excelFile, string fileName);
        DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details);
    }
}
