using Microsoft.Extensions.DependencyInjection;
using IntoRdf.Services.TransformationServices.RdfGraphServices;
using IntoRdf.Services.DomReaderServices.ExcelDomReaderServices;
using IntoRdf.Services.TransformationServices.RdfTableBuilderServices;
using IntoRdf.Services.TransformationServices.SpreadsheetServices;
using IntoRdf.Services.TransformationServices.OntologyServices;
using IntoRdf.Services.TransformationServices.RecordService;

namespace IntoRdf.Services.DependencyInjection;
internal static class SetupServices
{
    internal static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        /*        IExcelDomReaderService excelDomReaderService, 
        IExcelRdfTableBuilderService excelTableBuilderService,
        IRdfGraphService rdfGraphService)
        */
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