using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.FusekiServices;
using Services.TransformerServices;
using Services.RdfServices;
using Services.TransformationServices.RecordService;
using Services.TransformationServices.RdfGraphServices;
using Services.TransformationServices.RdfTableBuilderServices;
using Services.TransformationServices.SpreadsheetServices;
using Services.TransformationServices.OntologyServices;
using Services.RecordServices;
using Repositories.OntologyRepository;
using Repositories.RecordRepository;


namespace Services.DependencyInjection;
public static class SetupServices
{
    public static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        services.AddTransient<IFusekiQueryService, FusekiQueryService>();
        services.AddTransient<IFusekiService, FusekiService>();
        services.AddTransient<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddTransient<IRdfGraphService, RdfGraphService>();
        services.AddTransient<IRdfService, RdfService>();
        services.AddTransient<IExcelRdfTableBuilderService, ExcelRdfTableBuilderService>();
        services.AddTransient<ISpreadsheetService, SpreadsheetService>();
        services.AddTransient<IOntologyService, OntologyService>();
        services.AddTransient<IRecordTransformationService, RecordTransformationService>();
        services.AddTransient<IRecordService, RecordService>();
        services.AddTransient<ITransformerService, TransformerService>();
        return services;
    }

    public static IServiceCollection AddSplinterRepositories(this IServiceCollection services)
    {
        services.AddTransient<IOntologyRepository, OntologyRepository>();
        services.AddTransient<IRecordRepository, RecordRepository>();
        return services;
    }

}