namespace IntoRdf.Public.Models;

public class SpreadsheetDetails
{
    public SpreadsheetDetails(string sheetName, int headerRow, int dataStartRow, int startColumn)
    {
        SheetName = sheetName;
        HeaderRow = headerRow;
        DataStartRow = dataStartRow;
        StartColumn = startColumn;
    }

    public string SheetName { get;  }
    public int HeaderRow { get; }
    public int DataStartRow { get;  }
    public int StartColumn { get; }
    public int DataEndRow { get; set; }
    public int EndColumn { get; set; }
}