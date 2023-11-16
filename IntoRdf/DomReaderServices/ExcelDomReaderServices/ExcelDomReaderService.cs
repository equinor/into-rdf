using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Data;
using System.Text.RegularExpressions;
using IntoRdf.Models;

namespace IntoRdf.DomReaderServices.ExcelDomReaderServices;

internal class ExcelDomReaderService : IExcelDomReaderService
{
    List<string> invalidFormulaCellPositions = new List<string>();
    List<int> emptyHeaderCellPositions = new List<int>();
    List<string>cellsWithDataNoHeader = new List<string>();

    public DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails spreadsheetDetails)
    {
        if (spreadsheetDetails.SheetName == null) throw new InvalidOperationException("Failed to transform spreadsheet. Missing spreadsheet name in SpreadsheetContext");

        var doc = SpreadsheetDocument.Open(excelFile, false);
        var workbookPart = doc.WorkbookPart ?? throw new ArgumentNullException("The file does not contain a workbook");
        var worksheetPart = GetWorksheetPart(doc, spreadsheetDetails.SheetName);

        var headerRow = GetHeaderRow(worksheetPart, workbookPart, spreadsheetDetails);
        CheckIfMissingColumnHeaders(headerRow);
        var dataRows = GetDataRows(worksheetPart, workbookPart, headerRow, spreadsheetDetails);
        CheckForEmptyFormulaCells(invalidFormulaCellPositions);
        if (emptyHeaderCellPositions.Count>3  || cellsWithDataNoHeader.Count > 5)
        {
            var columnLetters = emptyHeaderCellPositions.Select(columnIndex => GetExcelColumnLetter(columnIndex));
            throw new Exception($"No column header at {string.Join(", ", columnLetters)}. but data is present in column.");
        }
        else if (cellsWithDataNoHeader.Any())
        {
            throw new Exception($"The cell at position(s) {string.Join(", ", cellsWithDataNoHeader)} contains data but column has no header.");
        }



        var data = CreateDataTable(spreadsheetDetails.DataStartRow, headerRow, dataRows, spreadsheetDetails.SheetName);

        return data;
    }

    private List<string> GetHeaderRow(WorksheetPart worksheetPart, WorkbookPart workbookPart, SpreadsheetDetails spreadsheetDetails)
    {
        var headerRow = worksheetPart
            .Worksheet
            .Descendants<Row>()
            .FirstOrDefault(r => (r.RowIndex ?? 0) == spreadsheetDetails.HeaderRow);

        if (headerRow == null)
        {
            throw new Exception($"Looks like the specified header row, row {spreadsheetDetails.HeaderRow}, is empty.");
        }

        return GetCompleteRow(workbookPart, headerRow, spreadsheetDetails.StartColumn, spreadsheetDetails.EndColumn).ToList();
    }

    private List<List<string>> GetDataRows(WorksheetPart worksheetPart,
                                            WorkbookPart workbookPart,
                                            List<string> headerRow,
                                            SpreadsheetDetails spreadsheetDetails)
    {
        var endRow = spreadsheetDetails.DataEndRow ?? int.MaxValue;

        var dataRows = worksheetPart
            .Worksheet
            .Descendants<Row>()
            .Where(r => (r.RowIndex ?? 0) >= spreadsheetDetails.DataStartRow && (r.RowIndex ?? 0) <= endRow);

        return dataRows
            .Where(r => r.Descendants<Cell>().Any())
            .Select(row => GetCompleteRow(workbookPart, row, spreadsheetDetails.StartColumn, spreadsheetDetails.StartColumn + headerRow.Count).ToList())
            .ToList();
    }

    private IEnumerable<string> GetCompleteRow(WorkbookPart wbPart, Row row, int startColumn, int? endColumn)
    {
        // When looping through an excel sheet reperesented as xml we need to keep two indexes distinct:
        //      - what we refer to as `excelIndex`, this is simply A = 1, B = 2 and so on
        //      - what we refer to as `xmlIndex`, this is the current child number of the `row` we are passed
        // These two differs:
        //      - xmlIndex is 0-indexed (starting at 0) while excelIndex is 1-indexed (starting at 1)
        //      - if we leave excelcells empty they will not be a part of the xml. This means that if
        //        some cells are left empty in one row and not the header row, a trivial go through will
        //        cause the row to be misalligned with the header, thereby the corresponding data table
        //        will use incorrect headers to describe the data. The property `CellReference` does
        //        however give us the excelIndex which we can use to yield empty cells until the two
        //        indexes matches again. See:
        //        https://stackoverflow.com/questions/36100011/c-sharp-open-xml-empty-cells-are-getting-skipped-while-getting-data-from-excel


        var descendants = row.Descendants<Cell>();
        var xmlCount = descendants.Count();

        var excelEndColumn = endColumn ?? int.MaxValue;

        var excelIndex = startColumn;
        var xmlIndex = startColumn - 1;
        for (; excelIndex <= excelEndColumn && xmlIndex < xmlCount; excelIndex++, xmlIndex++)
        {
             
            var cell = descendants.ElementAt(xmlIndex) ?? throw new InvalidOperationException("Spreadsheet does not contain cell");
            var reference = cell.CellReference?.ToString()?.ToLower() ?? throw new InvalidOperationException("Spreadsheet cell does not contain cell reference");
            var columnLetters = Regex.Match(reference, @"[a-z]+").Value;
            var columnNumber = NumberFromExcelColumn(columnLetters);

            for (; excelIndex <= excelEndColumn && excelIndex < columnNumber; excelIndex++)
            {
                yield return string.Empty;
            }
            var value = GetCellValue(cell, wbPart);
            if(emptyHeaderCellPositions.Contains(excelIndex) && value!="")
            {
                cellsWithDataNoHeader.Add($"{reference}");
            }

            yield return value;
          }
    }

    private static WorksheetPart GetWorksheetPart(SpreadsheetDocument doc, string sheetName)
    {
        var book = doc?
            .WorkbookPart?
            .Workbook ?? throw new InvalidOperationException("Spreadsheet does not contain workbook");

        var sheets = book
            .Descendants<Sheet>();

        var sheet = sheets
            .FirstOrDefault(s => s.Name?.ToString()?.Contains(sheetName) ?? false);

        //Handling nullable warning for GetPartById
        var sheetId = String.Empty;
        if (sheet?.Id is not null)
        {
            sheetId = sheet.Id;
        }

        if (string.IsNullOrEmpty(sheetId))
        {
            var otherSheetNames = sheets.Select(s => s.Name);
            throw new InvalidOperationException($"Did not find sheet with name {sheetName} among [{string.Join(",", otherSheetNames)}]");
        }

        var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheetId);

        return worksheetPart;
    }

    private string GetCellValue(Cell cell, WorkbookPart wbPart)
    {
        if (cell == null)
        {
            return "";
        }
        string value = cell.InnerText;
        
        if (cell.CellFormula is not null && cell.CellValue.Text=="0")
        {
            string cellReference = cell.CellReference.InnerText;
            string columnName = Regex.Replace(cellReference, "[0-9]", "");
            int rowNumber = int.Parse(Regex.Replace(cellReference, "[^0-9]", ""));
            invalidFormulaCellPositions.Add($"{columnName}{rowNumber}");
        }

        if (cell.DataType != null)
        {
            switch (cell.DataType.Value)
            {
                case CellValues.SharedString:
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    if (stringTable != null)
                    {
                        value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                    }
                    break;

                case CellValues.Boolean:
                    switch (value)
                    {
                        case "0":
                            value = "false";
                            break;
                        default:
                            value = "true";
                            break;
                    }
                    break;
                case CellValues.String:
                    if (cell.CellValue != null)
                    {
                        value = cell.CellValue.InnerText;
                    }

                    break;
            }
        }
        else if (cell.CellValue != null)
        {
            // For formulas
            value = cell.CellValue.InnerText;
        }

        return value;
    }

    private static int NumberFromExcelColumn(string column)
    {
        int retVal = 0;
        string col = column.ToUpper();
        for (int iChar = col.Length - 1; iChar >= 0; iChar--)
        {
            char colPiece = col[iChar];
            int colNum = colPiece - 64;
            retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
        }
        return retVal;
    }

    private static DataTable CreateDataTable(int startRow, List<string> headers, List<List<string>> data, string sheetName)
    {
        var inputDataTable = new DataTable();
        inputDataTable.TableName = sheetName ?? "InputData";

        foreach (var header in headers)
        {
            inputDataTable.Columns.Add(header);
        }

        for (int i = 0; i < data.Count(); i++)
        {
            var row = inputDataTable.NewRow();

            var dataColumns = data[i].Count > headers.Count ? headers.Count : data[i].Count;
            for (int j = 0; j < dataColumns; j++)
            {
                row[j] = data[i][j];
            }
            inputDataTable.Rows.Add(row);
        }

        return inputDataTable;
    }

    private void CheckForEmptyFormulaCells(List<String> invalidFormulaCellPositions)
    {
        if (invalidFormulaCellPositions.Any())
        {
            throw new Exception($"The cell at position(s) {string.Join(", ", invalidFormulaCellPositions)} contains a formula but has no value.");
        }
    }
    private void CheckIfMissingColumnHeaders(List<string> headerRow)
    {
        //Remove trailing empty cells until data is present
        for (int i = headerRow.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrEmpty(headerRow[i]))
            {
                break;
            }

            headerRow.RemoveAt(i);
        }

        for (int i = 0; i < headerRow.Count; i++)
        {
            if (string.IsNullOrEmpty(headerRow[i]))
            {
                emptyHeaderCellPositions.Add(i+1);
            }
        }
    }
    private string GetExcelColumnLetter(int columnNumber)
    {
        int dividend = columnNumber;
        string columnLetter = string.Empty;

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnLetter = Convert.ToChar(65 + modulo).ToString() + columnLetter;
            dividend = (dividend - modulo) / 26;
        }

        return columnLetter;
    }

}
