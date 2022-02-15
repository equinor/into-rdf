using Doc2Rdf.Library;
using Doc2Rdf.Library.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/melexcel/upload", async (
    HttpRequest req,
    string projectCode,
    int revision,
    DateTime revisionDate,
    string contractor,
    DocumentType documentType,
    int headerRow,
    string sheetName,
    int startRow,
    int endRow,
    int startColumn,
    int endColumn,
    bool isTransposed) =>
{
    if (!req.HasFormContentType)
    {
        return Results.BadRequest();
    }

    var form = await req.ReadFormAsync();
    var excelFile = form.Files[0];

    var stream = new MemoryStream();
    await excelFile.CopyToAsync(stream);

    var details = new SpreadsheetDetails
    {
        ProjectCode = projectCode,
        Revision = revision,
        RevisionDate = revisionDate,
        Contractor = contractor,
        DocumentType = documentType,
        HeaderRow = headerRow,
        SheetName = sheetName,
        DataStartRow = startRow,
        DataEndRow = endRow,
        StartColumn = startColumn,
        EndColumn = endColumn,
        IsTransposed = isTransposed,
    };

    var doc = SpreadsheetDocument.Open(stream, false);
    if (doc?.WorkbookPart == null) throw new Exception();
    var sheet = doc
        .WorkbookPart
        .Workbook
        .Descendants<Sheet>()
        .First(s => sheetName == s.Name);

    return Results.Ok();
}).Accepts<IFormFile>("multipart/form-data").Produces(200);

app.Run();
