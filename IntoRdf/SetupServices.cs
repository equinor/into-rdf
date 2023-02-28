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
    internal static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        services.AddTransient<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddTransient<IRdfGraphService, RdfGraphService>();
        services.AddTransient<IExcelRdfTableBuilderService, ExcelRdfTableBuilderService>();
        services.AddTransient<ISpreadsheetService, SpreadsheetService>();
        services.AddTransient<IOntologyService, OntologyService>();
        services.AddTransient<IRecordTransformationService, RecordTransformationService>();
        services.AddTransient<ITransformerService, TransformerService>();
        return services;
    }
}