using Microsoft.Extensions.DependencyInjection;
using IntoRdf.TransformationServices.RdfGraphServices;
using IntoRdf.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.TransformationServices.RdfTableBuilderServices;
using IntoRdf.TransformationServices.SpreadsheetServices;
using IntoRdf.TransformationServices.OntologyServices;
using IntoRdf.TransformationServices.RecordService;

namespace IntoRdf.DependencyInjection;
internal static class SetupServices
{
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddTransient<IRdfGraphService, RdfGraphService>();
        services.AddTransient<IDataTableProcessor, DataTableProcessor>();
        services.AddTransient<ISpreadsheetService, SpreadsheetService>();
        services.AddTransient<IOntologyService, OntologyService>();
        services.AddTransient<IRecordTransformationService, RecordTransformationService>();
        services.AddTransient<ITransformerService, TransformerService>();
        services.AddTransient<ITabularJsonTransformationService, TabularJsonTransformationService>();
        return services;
    }
}