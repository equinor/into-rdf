using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.FusekiServices;
using Services.RecordServices;
using Services.GraphParserServices;
using Services.ProvenanceServices;
using Services.RdfServices;
using Services.RdfServices.XmlServives;
using Services.TieMessageServices;
using Services.TransformationServices.DatabaseTransformationServices;
using Services.TransformationServices.RdfGraphServices;
using Services.TransformationServices.RdfPreprocessingServices;
using Services.TransformationServices.RdfTableBuilderServices;
using Services.TransformationServices.RdfTransformationServices;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.TransformationServices.SourceToOntologyConversionService;
using Services.TransformationServices.XMLTransformationServices;
using Services.TransformationServices.XMLTransformationServices.Converters;
using Services.CommonlibServices;
using Services.CommonLibToRdfServices;
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
        services.AddScoped<IDatabaseTransformationService, ShipweightTransformationService>();
        services.AddScoped<IFusekiQueryService, FusekiQueryService>();
        services.AddScoped<IFusekiService, FusekiService>();
        services.AddScoped<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddScoped<IProvenanceService, ProvenanceService>();
        services.AddScoped<IRdfGraphService, RdfGraphService>();
        services.AddScoped<IRdfPreprocessingService, RdfPreprocessingService>();
        services.AddScoped<IRdfService, RdfService>();
        services.AddScoped<IRdfTableBuilderFactory, RdfTableBuilderFactory>();
        services.AddScoped<IRdfTableBuilderService, ExcelRdfTableBuilderService>();
        services.AddScoped<IRdfTableBuilderService, ShipweightRdfTableBuilderService>();
        services.AddScoped<IRdfTableBuilderService, CommonLibTableBuilderService>();
        services.AddScoped<IRdfTransformationService, RdfTransformationService>();
        services.AddScoped<ISpreadsheetTransformationService, SpreadsheetTransformationService>();
        services.AddScoped<ISourceToOntologyConversionService, SourceToOntologyConversionService>();
        services.AddScoped<ITieMessageService, TieMessageService>();
        services.AddScoped<ICommonLibService, CommonlibService>();
        services.AddScoped<ICommonLibTransformationService, CommonLibTransformationService>();
        services.AddScoped<ICommonLibToRdfService, CommonLibToRdfService>();
        services.AddScoped<IRevisionTrainService, RevisionTrainService>();
        services.AddScoped<IRevisionTrainValidator, RevisionTrainValidator>();
        services.AddScoped<IRecordService, RecordService>();
        services.AddScoped<IGraphParser, GraphParser>();
        services.AddScoped<IRevisionService, RevisionService>();        
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