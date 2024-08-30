using Microsoft.Extensions.DependencyInjection;
using IntoRdf.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.TransformationServices;
using IntoRdf.TransformationServices.SpreadsheetServices;

namespace IntoRdf.DependencyInjection;
internal static class SetupServices
{
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddTransient<IRdfAssertionService, RdfAssertionService>();
        services.AddTransient<IDataTableProcessor, DataTableProcessor>();
        services.AddTransient<ISpreadsheetService, SpreadsheetService>();
        services.AddTransient<ICsvService, CsvService>();
        services.AddTransient<ITransformerService, TransformerService>();
        services.AddTransient<ITabularJsonTransformationService, TabularJsonTransformationService>();
        return services;
    }
}