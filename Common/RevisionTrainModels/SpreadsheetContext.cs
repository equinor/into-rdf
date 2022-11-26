namespace Common.RevisionTrainModels
{
    public class SpreadsheetContext
    {
        public SpreadsheetContext(string sheetName, int headerRow, int dataStartRow, int startColumn)
        {
            SheetName = sheetName;
            HeaderRow = headerRow;
            DataStartRow = dataStartRow;
            StartColumn = startColumn;
            IsTransposed = false;
        }
        public string SheetName { get; set; }
        public int HeaderRow { get; set; }
        public int DataStartRow { get; set; }
        public int StartColumn { get; set; }
        public int DataEndRow { get; set; } 
        public int EndColumn { get; set; }
        public bool IsTransposed {get; set;}
    }
}