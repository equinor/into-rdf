using Common.ProvenanceModels;

namespace Common.SpreadsheetModels;

public class SpreadsheetInfo
{
    public string? DocumentProjectId { get; set; }
    public int Revision { get; set; }
    public string? RevisionName { get; set; }
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

    public RevisionRequirement GetRevisionRequirements()
    {
        if (DocumentProjectId == null || RevisionName == null || RevisionDate == DateTime.MinValue)
        {
            throw new InvalidOperationException("Not sufficient spreadsheet info to create revision requirements");
        }

        return new RevisionRequirement(DocumentProjectId, RevisionName, RevisionDate);
    }

    public bool TryValidate(out InvalidOperationException exception)
    {

        if (FileName == null)
        {
            exception =  new InvalidOperationException("Spreadsheet details are missing. Start by including 'FileName' in request body");
            return false;
        }

        if (DocumentProjectId == null)
        {
            exception = new InvalidOperationException(DetailsErrorMessage(nameof(DocumentProjectId)));
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
}
