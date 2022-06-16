﻿using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Doc2Rdf.Library.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Doc2Rdf.Library.IO
{
    public class DomMelReader : IMelReader
    {
        private readonly ILogger<DomMelReader> _logger;

        public DomMelReader(ILogger<DomMelReader> logger)
        {
            _logger = logger;
        }
        public SpreadsheetInfo GetSpreadsheetInfo(Stream excelFile, string fileName)
        {
            var doc = SpreadsheetDocument.Open(excelFile, false);

            var workbookPart = doc.WorkbookPart ?? throw new ArgumentNullException($"The file {fileName} does not contain a workbook");
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

            return new SpreadsheetInfo
            {
                HeaderRow = int.Parse(rows["HeaderRow"]),
                StartColumn = NumberFromExcelColumn(rows["StartColumn"]),
                EndColumn = NumberFromExcelColumn(rows["EndColumn"]),
                DataStartRow = int.Parse(rows["DataStartRow"]),
                DataEndRow = int.Parse(rows["DataEndRow"]),
                Contractor = rows["Contractor"],
                DataSource = rows.TryGetValue("DataType", out string? dataType) ? dataType : DataSourceType.Unknown(),
                IsTransposed = bool.Parse(rows.TryGetValue("IsTransposed", out string? isTransposed) ? isTransposed : "false"),
                ProjectCode = rows["ProjectCode"],
                Revision = int.Parse(rows["Revision"].TrimStart('0')),
                RevisionDate = DateTime.Parse(rows["RevisionDate"]),
                SheetName = rows["SheetName"],
                FileName = Path.GetFileName(fileName)
            };
        }

        public DataTable GetSpreadsheetData(Stream excelFile, SpreadsheetDetails details)
        {
            var doc = SpreadsheetDocument.Open(excelFile, false);

            var workbookPart = doc.WorkbookPart ?? throw new ArgumentNullException("The file does not contain a workbook");
            var worksheetPart = GetWorksheetPart(doc, details.SheetName);

            var headerRow = GetHeaderRow(worksheetPart, workbookPart, details);

            _logger.LogDebug("<DomMelReader> - GetSpreadsheetData: - Created header row with {nbOfColumns} columns", headerRow.Count);

            var dataRows = GetDataRows(worksheetPart, workbookPart, headerRow, details);

            _logger.LogDebug("<DomMelReader> - GetSpreadsheetData: - Created dataset with {nbOfRows} rows", dataRows.Count);

            var data = CreateDataTable(details.DataStartRow, headerRow, dataRows, details.SheetName);

            _logger.LogDebug("<DomMelReader> - GetSpreadsheetData: - Created table with {nbOfRows} rows", data.Rows.Count);
            return data;
        }

        private List<string> GetHeaderRow(WorksheetPart worksheetPart, WorkbookPart workbookPart, SpreadsheetDetails details)
        {
            var columnSkip = details.StartColumn - 1;
            var columnTake = details.EndColumn - columnSkip;
            
            var completeHeaderRow = worksheetPart
                .Worksheet
                .Descendants<Row>()
                .First(r => (r.RowIndex ?? 0) == details.HeaderRow)
                .Skip(columnSkip);

            var trimmedHeaderRow = columnTake > 0 ? completeHeaderRow.Take(columnTake) : completeHeaderRow;

            return trimmedHeaderRow
                .Select(xmlElement => GetCellValue((Cell)xmlElement, workbookPart))
                .Where(xmlElement => xmlElement != "")
                .ToList();
        }

        private List<List<string>> GetDataRows(WorksheetPart worksheetPart,
                                                WorkbookPart workbookPart, 
                                                List<string> headerRow, 
                                                SpreadsheetDetails details)
        {
            var rowSkip = details.DataStartRow - 1;
            var rowTake = details.DataEndRow - rowSkip;

            var tagNumberIndex = headerRow.FindIndex(x => x == "Tag Number");

            var completeDataRows = worksheetPart
                .Worksheet
                .Descendants<Row>()
                .Skip(rowSkip);
            
            var trimmedDataRows = rowTake > 0 ? completeDataRows.Take(rowTake) : completeDataRows;

            return trimmedDataRows
                .Where(row => ValidRow(row, tagNumberIndex))
                .Select(row => GetCompleteRow(workbookPart, row, details.StartColumn, headerRow.Count).ToList())
                .ToList();
        }

        private bool ValidRow(Row row, int tagNumberIndex)
        {
            var descendants = row.Descendants<Cell>();

            if (descendants.Count() < tagNumberIndex)
            {
                return false;
            }
            
            var cell = descendants.ElementAt(tagNumberIndex);
            
            return cell.CellValue != null;
        }

        private IEnumerable<string> GetCompleteRow(WorkbookPart wbPart, Row row, int startColumn, int endColumn)
        {
            var descendants = row.Descendants<Cell>();

            // `i` in the loop below should always be the actual column I am looking at in excel
            // (A = 1, B = 2, ...)
            // maintaining a negative offset to pass to ElementAt which is zero-indexed. If we hit
            // empty cell they will not be stored in descendants and the difference between actual
            // excel columns and what we are iterating through using ElementAt will increase.
            var offset = -1;

            for (int i = startColumn; i <= endColumn && i + offset < descendants.Count(); i++)
            {
                var cell = descendants.ElementAt(i + offset) ?? throw new InvalidOperationException("Spreadsheet does not contain cell");
                var reference = cell.CellReference?.ToString()?.ToLower() ?? throw new InvalidOperationException("Spreadsheet cell does not contain cell reference");
                var columnLetters = Regex.Match(reference, @"[a-z]+").Value;
                var columnNumber = NumberFromExcelColumn(columnLetters);

                // row.Descendants<Cell> will not give us empty cells so if we notice skip in
                // cell reference yield empty cells until we catch up. See
                // https://stackoverflow.com/questions/36100011/c-sharp-open-xml-empty-cells-are-getting-skipped-while-getting-data-from-excel
                for (; i < columnNumber; i++)
                {
                    offset--;
                    yield return string.Empty;
                }

                yield return GetCellValue(cell, wbPart);
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
                .First(s => s.Name?.ToString()?.Contains(sheetName) ?? false);

            //Handling nullable warning for GetPartById
            var sheetId = String.Empty;
            if (sheet?.Id is not null)
            {
                sheetId = sheet.Id;
            }

            if (string.IsNullOrEmpty(sheetId))
            {
                throw new InvalidOperationException($"Spreadsheet does not contain sheet {sheetName}");
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

            inputDataTable.Columns.Add("id");

            foreach (var header in headers)
            {
                inputDataTable.Columns.Add(header);
            }

            for (int i = 0; i < data.Count(); i++)
            {
                var row = inputDataTable.NewRow();
                row[0] = (startRow + i).ToString();

                var dataColumns = data[i].Count > headers.Count ? headers.Count : data[i].Count;
                for (int j = 1; j <= dataColumns; j++)
                {
                    row[j] = data[i][j - 1];
                }
                inputDataTable.Rows.Add(row);
            }
            
            return inputDataTable;
        }
    }
}
