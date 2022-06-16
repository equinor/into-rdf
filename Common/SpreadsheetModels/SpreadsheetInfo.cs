namespace Common.SpreadsheetModels;

public class SpreadsheetInfo
{
    public string? ProjectCode { get; set; }
    public int Revision { get; set; }
    public bool IsTransposed { get; set; }
    public DateTime RevisionDate { get; set; }
    public string? Contractor { get; set; }
    public string? DataSource { get; set; }
    public string? SheetName { get; set; }
    public int HeaderRow { get; set; }
    public int DataStartRow { get; set; }
    public int DataEndRow { get; set; }
    public int StartColumn { get; set; }
    public int EndColumn { get; set; }
    public string? FileName { get; set; }

    public SpreadsheetDetails GetSpreadSheetDetails()
    {
        if (SheetName == null || (HeaderRow == 0 && DataStartRow == 0))
        {
            throw new InvalidDataException("Spreadsheet info does not contain sufficient details");
        }
        return new SpreadsheetDetails(
            this.SheetName,
            this.HeaderRow,
            this.DataStartRow,
            this.StartColumn,
            this.DataEndRow,
            this.EndColumn,
            this.IsTransposed);
    }
}
