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

    [CommandOption("-d|--data-start-row")]
    public int DataStartRow { get; set; } = 2;

    [CommandOption("--data-end-row")]
    public int DataEndRow { get; set; } = int.MaxValue;

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

    [Description("Name of the column to be used for rdfs:label")]
    [CommandOption("--label-column")]
    public string? LabelColumn { get; set; } = null;

    [Description("Name of the column to be used for skos:definition")]
    [CommandOption("--definition-column")]
    public string? DefinititionColumn { get; set; } = null;


    [Description("jsonld, turtle or trig")]
    [CommandOption("-f|--output-format")]
    public RdfFormat OutputFormat { get; set; } = RdfFormat.Turtle;

    [Description("A colon separated entry indicating target and url segment of identifier column. For example -i 'SomeId:IdSegment'")]
    [CommandOption("-i |--identifier-segment")]
    public string? IdSegment { get; set; } = null;

    [Description("A list of colon separated entries indicating target and url segment. For example -t 'SomeField:SomeField' -t 'SomeOtherField:SomeOtherField'")]
    [CommandOption("-t |--target-path-segment")]
    public string[] TargetPathSegments { get; set; } = new string[0];

    [Description("Do custom url encoding i.e. ² -> SQUARED', ³ -> CUBED, and ° -> DEGREES'")]
    [CommandOption("-e |--encode")]
    public bool CustomEncoding { get; set; } = false;
}

internal class TransformExcelCommand : Command<TransformExcelSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] TransformExcelSettings settings)
    {
        var details = new SpreadsheetDetails(settings.SheetName, settings.PredicateRow, settings.DataStartRow, settings.StartColumn)
        {
            DataEndRow = settings.DataEndRow,
            EndColumn = settings.EndColumn,
        };
        TargetPathSegment? idSegment = GetIdSegment(settings);
        TargetPathSegment? labelSegment = GetLabelSegment(settings.LabelColumn);
        TargetPathSegment? definitionSegment = GetDefinitionSegment(settings.DefinititionColumn);

        var columnConfigs = settings.TargetPathSegments
                .Select(raw => GetSegment(raw, "--target-path-segment"))
                .Concat([labelSegment, definitionSegment])
                .Where((s) => s != null)
                .ToList();

        var CustomEncoding = settings.TargetPathSegments
            .Select(raw => GetCustomEncoding(raw, "--encode"))
            .ToDictionary((pair) => pair.Key, (pair) => pair.Value);

        var customEncoding = new Dictionary<string, string> {
                {"\u00b2", "SQUARED" },
                {"\u00b3", "CUBED" },
                {"\u00b0", "DEGREES" },
            };

        var transformationDetails = new TransformationDetails(
            new Uri(settings.BaseUri),
            new Uri(settings.BaseUri),
            idSegment,
            [.. columnConfigs],
            settings.OutputFormat,
            settings.CustomEncoding ? customEncoding : null
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
                                settings.DataStartRow,
                                settings.StartColumn
                            )
        {
            EndColumn = settings.EndColumn,
            DataEndRow = settings.DataEndRow,
        };
        return transformer.TransformSpreadsheet(spreadsheetDetails, transformationDetails, inputStream);
    }

    private static TargetPathSegment? GetIdSegment(TransformExcelSettings settings)
    {
        if (string.IsNullOrEmpty(settings.IdSegment)) { return null; }
        return GetSegment(settings.IdSegment, "--identifier-segment");
    }

    private static TargetPathSegment? GetLabelSegment(string? columnName)
    {
        if (string.IsNullOrEmpty(columnName)) { return null; }
        return new TargetPathSegment(columnName, null, "http://www.w3.org/2000/01/rdfschema#label");
    }

    private static TargetPathSegment? GetDefinitionSegment(string? columnName)
    {
        if (string.IsNullOrEmpty(columnName)) { return null; }
        return new TargetPathSegment(columnName, null, "http://www.w3.org/2004/02/skos/core#definition");
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

    private static KeyValuePair<string, string> GetCustomEncoding(string encoding, string paramNameForDebug)
    {
        var split = encoding.Split(":");
        if (split.Length != 2)
        {
            throw new Exception($"Expected a ':' in {paramNameForDebug}");
        }
        return new KeyValuePair<string, string>(split[0], split[1]);
    }
}