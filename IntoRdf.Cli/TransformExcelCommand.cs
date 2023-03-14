using IntoRdf.Public.Models;
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

    [Description("jsonld, turtle or trig")]
    [CommandOption("-f|--output-format")]
    public RdfFormat RdfFormat { get; set; } = RdfFormat.Turtle;

    [Description("A list of colon separated entries indicating target and url segment. Identifier column must be the first entry. For example -t 'SomeId:IdSegment' -t 'SomeOtherField:SomeField'")]
    [CommandOption("-t |--target-path-segment")]
    public string[] TargetPathSegments { get; set; } = new string[0];
}

internal class TransformExcelCommand : Command<TransformExcelSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] TransformExcelSettings settings)
    {
        var details = new SpreadsheetDetails(settings.SheetName, settings.PredicateRow, settings.DataRow, settings.StartColumn) { EndColumn = settings.EndColumn};
        var segments = settings.TargetPathSegments
                .Select(raw => raw.Split(":"))
                .Select(array => new TargetPathSegment(array[0], array[1]))
                .ToList();

        var transformationDetails = new TransformationDetails(
            new Uri(settings.BaseUri),
            new Uri(settings.BaseUri), 
            segments.FirstOrDefault(),
            segments,
            settings.RdfFormat
        );

        var inputStream = Console.OpenStandardInput();
        var transformer = new TransformerService();

        try
        {
            var rdf = transformer.TransformSpreadsheet(details, transformationDetails, inputStream);
            Console.WriteLine(rdf);
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        return 0;
    }
}