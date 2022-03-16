using Doc2Rdf.Library.Models;
using Doc2Rdf.Library.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Doc2Rdf.Library.IO
{
    internal class DomMelReader : IMelReader
    {
        public SpreadsheetDetails GetSpreadsheetDetails(FileStream excelFile)
        {
            var doc = SpreadsheetDocument.Open(excelFile, false);

            var workbookPart = doc.WorkbookPart;
            var worksheetPart = GetWorksheetPart(doc, "Provenance");

            var rows = worksheetPart
                .Worksheet
                .Descendants<Row>()
                .Take(13)
                .Select(row => GetCompleteRow(workbookPart, row, 1, 2).ToList())
                .ToDictionary(
                    pair => pair[0],
                    pair => pair[1]
                );

            return new SpreadsheetDetails
            {
                HeaderRow = int.Parse(rows["HeaderRow"]),
                StartColumn = NumberFromExcelColumn(rows["StartColumn"]),
                EndColumn = NumberFromExcelColumn(rows["EndColumn"]),
                DataStartRow = int.Parse(rows["DataStartRow"]),
                DataEndRow = int.Parse(rows["DataEndRow"]),
                Contractor = rows["Contractor"],
                DataSource = Enum.Parse<DataSource>(rows.TryGetValue("DataType", out string dataType) ? dataType : "NA"),
                IsTransposed = bool.Parse(rows.TryGetValue("IsTransposed", out string isTransposed) ? isTransposed : "false"),
                ProjectCode = rows["ProjectCode"],
                Revision = int.Parse(rows["Revision"].TrimStart('0')),
                RevisionDate = DateTime.Parse(rows["RevisionDate"]),
                SheetName = rows["SheetName"],
                FileName = Path.GetFileName(excelFile.Name)
            };
        }

        public DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details)
        {
            var doc = SpreadsheetDocument.Open(excelFile, false);

            var workbookPart = doc.WorkbookPart;
            var worksheetPart = GetWorksheetPart(doc, details.SheetName);

            var columnSkip = details.StartColumn - 1;
            var columnTake = details.EndColumn - columnSkip;

            var headerRow = worksheetPart
                .Worksheet
                .Descendants<Row>()
                .First(r => int.Parse(r.RowIndex) == details.HeaderRow)
                .Skip(columnSkip)
                .Take(columnTake)
                .Select(xmlElement => GetCellValue((Cell) xmlElement, workbookPart))
                .ToList();

            var rowSkip = details.DataStartRow - 1;
            var rowTake = details.DataEndRow - rowSkip;

            var tagNumberIndex = headerRow.FindIndex(x => x == "Tag Number");

            var dataRows = worksheetPart
                .Worksheet
                .Descendants<Row>()
                .Skip(rowSkip)
                .Take(rowTake)
                .Where(row => ValidRow(row, tagNumberIndex))
                .Select(row => GetCompleteRow(workbookPart, row, details.StartColumn, details.EndColumn).ToList())
                .ToList();

            var data = CreateDataTable(details.DataStartRow, headerRow, dataRows);
            return data;
        }

        private static bool ValidRow(Row row, int tagNumberIndex)
        {
            var decendants = row.Descendants<Cell>();
            var cell = decendants.ElementAt(tagNumberIndex);

            return cell.CellValue != null;
        }

        private static IEnumerable<string> GetCompleteRow(WorkbookPart wbPart, Row row, int startColumn, int endColumn)
        {
            var decendants = row.Descendants<Cell>();

            // `i` in the loop below should always be the actual column I am looking at in excel
            // (A = 1, B = 2, ...)
            // maintaining a negative offset to pass to ElementAt which is zero-indexed. If we hit
            // empty cell they will not be stored in decendants and the difference between actual
            // excel columns and what we are iterating through using ElementAt will increase.
            var offset = - 1;

            for (int i = startColumn; i <= endColumn && i + offset < decendants.Count(); i++)
            {
                var cell = decendants.ElementAt(i + offset);
                var reference = cell.CellReference.ToString().ToLower();
                var columnLetters = Regex.Match(reference, @"[a-z]+").Value;
                var columnNumber = NumberFromExcelColumn(columnLetters);

                // row.Decendants<Cell> will not give us empty cells so if we notice skip in
                // cell reference yield empty cells until we catch up. See
                // https://stackoverflow.com/questions/36100011/c-sharp-open-xml-empty-cells-are-getting-skipped-while-getting-data-from-excel
                for (; i<columnNumber; i++)
                {
                    offset--;
                    yield return string.Empty;
                }

                yield return GetCellValue(cell, wbPart);
            }
        }

        private static WorksheetPart GetWorksheetPart(SpreadsheetDocument doc, string sheetName)
        {
            var book = doc
                .WorkbookPart
                .Workbook;

            var sheets = book
                .Descendants<Sheet>();

            var sheet = sheets
                .First(s => s.Name == sheetName);

            var worksheetPart = (WorksheetPart) doc.WorkbookPart.GetPartById(sheet.Id);

            return worksheetPart;
        }

        private static string GetCellValue(Cell cell, WorkbookPart wbPart)
        {
            if (cell == null)
            {
                return "";
            }
            string value = cell.InnerText;
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
                        value = cell.CellValue.InnerText;
                        break;
                }
            } else if (cell.CellValue != null)
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

        private static DataTable CreateDataTable(int startRow, List<string> headers, List<List<string>> data)
        {
            var inputDataTable = new DataTable();

            inputDataTable.Columns.Add("id");

            foreach (var header in headers)
            {
                inputDataTable.Columns.Add(header);
            }

            for (int i = 0; i < data.Count(); i++)
            {
                var row = inputDataTable.NewRow();
                row[0] = (startRow + i).ToString();

                for (int j = 1; j <= data[i].Count; j++)
                {
                    row[j] = data[i][j-1]; 
                }
                inputDataTable.Rows.Add(row);
            }

            return inputDataTable;
        }
    }
}
