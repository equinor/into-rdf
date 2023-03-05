using IntoRdf.Public.Models;
using Spectre.Console.Cli;

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

    [CommandOption("-b|--base-uri")]
    public string BaseUri { get; set; } = "http://example.com";

    [CommandOption("-f|--output-format")]
    public RdfFormat RdfFormat { get; set; } = RdfFormat.Turtle;
}

internal class TransformExcelCommand : Command<TransformExcelSettings>
{
    public override int Execute(CommandContext context, TransformExcelSettings settings)
    {
        var details = new SpreadsheetDetails(settings.SheetName, settings.PredicateRow, settings.DataRow, settings.StartColumn);
        var transformationDetails = new TransformationDetails(new Uri(settings.BaseUri), new Uri(settings.BaseUri), new List<TargetPathSegment> {new TargetPathSegment("Name", "Person", true)});

        var inputStream = Console.OpenStandardInput();
        var transformer = new TransformerService();

        try
        {
            var rdf = transformer.TransformSpreadsheet(details, transformationDetails, inputStream, settings.RdfFormat);
            Console.WriteLine(rdf);
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        return 0;
    }
}