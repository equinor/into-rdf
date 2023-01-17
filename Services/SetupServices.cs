using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.FusekiServices;
using Controller.RecordController;
using Services.TransformerServices;
using Services.GraphParserServices;
using Services.RdfServices;
using Services.TieMessageServices;
using Services.TransformationServices.RecordService;
using Services.TransformationServices.RdfGraphServices;
using Services.TransformationServices.RdfTableBuilderServices;
using Services.TransformationServices.SpreadsheetServices;
using Services.TransformationServices.OntologyServices;
using Services.TransformationServices.XMLTransformationServices.Converters;
using Services.RevisionServices;
using Services.ValidationServices.RevisionTrainValidationServices;
using Repositories.OntologyRepository;
using Repositories.RecordRepository;
using Repositories.RecordContextRepository;
using Repositories.RevisionTrainRepository;

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
        services.AddTransient<ITieMessageService, TieMessageService>();
        services.AddTransient<IRevisionTrainService, RevisionTrainService>();
        services.AddTransient<IRevisionTrainValidator, RevisionTrainValidator>();
        services.AddTransient<IRecordService, RecordService>();
        services.AddTransient<IRecordController, RecordController>();
        services.AddTransient<ITransformerService, TransformerService>();
        services.AddTransient<IGraphParser, GraphParser>();
        services.AddTransient<IRevisionService, RevisionService>(); 
        return services;
    }

    public static IServiceCollection AddSplinterRepositories(this IServiceCollection services)
    {
        services.AddTransient<IOntologyRepository, OntologyRepository>();
        services.AddTransient<IRecordRepository, RecordRepository>();
        services.AddTransient<IRecordContextRepository, RecordContextRepository>();
        services.AddTransient<IRevisionTrainRepository, RevisionTrainRepository>();

        return services;
    }

}