using IntoRdf.Models;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace IntoRdf.Cli;

internal class TransformExcelSettings : CommandSettings
{
    [CommandOption("-s|--sheet-name")]
    public string SheetName { get; set; } = "Ark1";

    [CommandOption("-p|--predicate-row")]
    public int PredicateRow { get; set; } = 1;

    [CommandOption("-d|--data-row")]
    public int DataRow { get; set; } = 2;

    [CommandOption("-c|--start-column")]
    public int StartColumn { get; set; } = 1;

    [CommandOption("--end-column")]
    public int EndColumn { get; set; } = int.MaxValue;

    [Description("base uri of new generated uris, defaults to http://example.com")]
    [CommandOption("-b|--base-uri")]
    public string BaseUri { get; set; } = "http://example.com";

    [Description("inputformat, csv or xlsx, defaults to xlsx")]
    [CommandOption("--input-format")]
    public ExcelFormat InputFormat { get; set; } = ExcelFormat.Xlsx;

    [Description("jsonld, turtle or trig")]
    [CommandOption("-f|--output-format")]
    public RdfFormat OutputFormat { get; set; } = RdfFormat.Turtle;

    [Description("A colon separated entry indicating target and url segment of identifier column. For example -i 'SomeId:IdSegment'")]
    [CommandOption("-i |--identifier-segment")]
    public string? IdSegment { get; set; } = null;

    [Description("A list of colon separated entries indicating target and url segment. Identifier column must be the first entry. For example -t 'SomeField:SomeField' -t 'SomeOtherField:SomeOtherField'")]
    [CommandOption("-t |--target-path-segment")]
    public string[] TargetPathSegments { get; set; } = new string[0];
}

internal class TransformExcelCommand : Command<TransformExcelSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] TransformExcelSettings settings)
    {
        TargetPathSegment? idSegment = GetIdSegment(settings);

        var segments = settings.TargetPathSegments
                .Select(raw => GetSegment(raw, "--target-path-segment"))
                .ToList();

        var transformationDetails = new TransformationDetails(
            new Uri(settings.BaseUri),
            new Uri(settings.BaseUri),
            idSegment,
            segments.ToList(),
            settings.OutputFormat
        );

        var inputStream = Console.OpenStandardInput();
        var transformer = new TransformerService();

        try
        {
            var rdf = settings.InputFormat switch
            {
                ExcelFormat.Xlsx => HandleXlsx(settings, transformationDetails, inputStream, transformer),
                ExcelFormat.Csv => HandleCsv(settings, transformationDetails, inputStream, transformer),
                _ => throw new Exception("Unknown " + nameof(ExcelFormat))
            };
            Console.WriteLine(rdf);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return 0;
    }

     private static string HandleCsv(TransformExcelSettings settings, TransformationDetails transformationDetails, Stream inputStream, TransformerService transformer)
    {
        var csvDetails = new CsvDetails();
        return transformer.TransformCsv(csvDetails, transformationDetails, inputStream);
    }

    private static string HandleXlsx(TransformExcelSettings settings, TransformationDetails transformationDetails, Stream inputStream, TransformerService transformer)
    {
        var spreadsheetDetails = new SpreadsheetDetails(
                                settings.SheetName,
                                settings.PredicateRow,
                                settings.DataRow,
                                settings.StartColumn
                            )
        { EndColumn = settings.EndColumn };
        return transformer.TransformSpreadsheet(spreadsheetDetails, transformationDetails, inputStream);
    }

    private static TargetPathSegment? GetIdSegment(TransformExcelSettings settings)
    {
        if (string.IsNullOrEmpty(settings.IdSegment)) { return null; }
        return GetSegment(settings.IdSegment, "--identifier-segment");
    }

    private static TargetPathSegment GetSegment(string segment, string paramNameForDebug)
    {
        var split = segment.Split(":");
        if (split.Length != 2)
        {
            throw new Exception($"Expected a ':' in {paramNameForDebug}");
        }
        return new TargetPathSegment(split[0], split[1]);
    }
}