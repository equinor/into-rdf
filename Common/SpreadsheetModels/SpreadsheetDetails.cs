namespace Common.SpreadsheetModels;

public class SpreadsheetDetails
{
    public SpreadsheetDetails(
        string sheetName,
        int headerRow,
        int dataStartRow,
        int startColumn,
        int dataEndRow = 0,
        int endColumn = 0,
        bool isTransposed = false) 
    {
        SheetName = sheetName;
        HeaderRow = headerRow;
        DataStartRow = dataStartRow;
        DataEndRow = dataEndRow;
        StartColumn = startColumn;
        EndColumn = endColumn;
        IsTransposed = isTransposed;
    }

    public string SheetName { get; }
    public int HeaderRow { get; }
    public int DataStartRow { get; }
    public int DataEndRow { get; }
    public int StartColumn { get; }
    public int EndColumn { get; }
    public bool IsTransposed { get; }
}