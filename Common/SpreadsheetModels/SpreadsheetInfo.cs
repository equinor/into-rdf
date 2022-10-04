using Common.ProvenanceModels;

namespace Common.SpreadsheetModels;

public class SpreadsheetInfo
{
    public string? FacilityId { get; set; }
    public string DocumentProjectId { get; set; } = String.Empty;
    public int Revision { get; set; }
    public string? RevisionName { get; set; }
    public bool IsTransposed { get; set; }
    public DateTime RevisionDate { get; set; }
    public string Contractor { get; set; } = String.Empty;
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
        if (SheetName != null && HeaderRow < DataStartRow)
        {
            return new SpreadsheetDetails(
                this.SheetName,
                this.HeaderRow,
                this.DataStartRow,
                this.StartColumn,
                this.DataEndRow,
                this.EndColumn,
                this.IsTransposed);
        }

        if (DataSource == Common.ProvenanceModels.DataSource.Mel)
        {
            return GetDefaultSpreadsheetDetailsForMel();
        }

        if (DataSource == Common.ProvenanceModels.DataSource.LineList)
        {
            return GetDefaultSpreadsheetDetailsForLineList();
        }

        throw new InvalidDataException("Spreadsheet info does not contain sufficient details");
    }

    public RevisionRequirement GetRevisionRequirements()
    {
        if (FacilityId == null || FileName == null || RevisionName == null || RevisionDate == DateTime.MinValue)
        {
            throw new InvalidOperationException("Not sufficient spreadsheet info to create revision requirements");
        }

        var documentName = FileName
                            .Split("_")
                            .First()
                            .Split(".")
                            .First();

        return new RevisionRequirement(FacilityId, documentName, RevisionName, RevisionDate);
    }

    public bool TryValidate(out InvalidOperationException exception)
    {

        if (FileName == null)
        {
            exception =  new InvalidOperationException("Spreadsheet details are missing. Start by including 'FileName' in request body");
            return false;
        }

        if (FacilityId == null)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(FacilityId)));
            return false;
        }

        if (DataSource == null)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(DataSource)));
            return false;
        }

        if (SheetName == null)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(SheetName)));
            return false;
        }

        if (RevisionName == null)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(SheetName)));
            return false;
        }

        if (RevisionDate == DateTime.MinValue)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(RevisionDate)));
            return false;
        }

        exception = new InvalidOperationException();
        return true;
    }
    
        private string DetailsErrorMessage(string missingParam)
    {
        return $"{missingParam} is missing from spreadsheet details";
    }

    private SpreadsheetDetails GetDefaultSpreadsheetDetailsForMel()
    {
        const string sheetName = "MEL";
        const int headerRow = 6;
        const int dataStartRow = 8;
        const int startColumn = 1;

        return new SpreadsheetDetails(sheetName, headerRow, dataStartRow, startColumn);
    }

    private SpreadsheetDetails GetDefaultSpreadsheetDetailsForLineList()
    {
        const string sheetName = "Line";
        const int headerRow = 1;
        const int dataStartRow = 3;
        const int startColumn = 1;

        return new SpreadsheetDetails(sheetName, headerRow, dataStartRow, startColumn);
    }
}
