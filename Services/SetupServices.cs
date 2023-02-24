using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformerServices;
using Services.TransformationServices.RecordService;
using Services.TransformationServices.RdfGraphServices;
using Services.TransformationServices.RdfTableBuilderServices;
using Services.TransformationServices.SpreadsheetServices;
using Services.TransformationServices.OntologyServices;


namespace Services.DependencyInjection;
public static class SetupServices
{
    public static IServiceCollection AddSplinterServices(this IServiceCollection services)
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