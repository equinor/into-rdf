using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.MelDomReaderServices;
using Services.FusekiServices;
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
using Services.TransformationServices.XMLTransformationServices;
using Services.TransformationServices.XMLTransformationServices.Converters;

namespace Services.DependencyInjection;
public static class SetupServices
{
    public static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseTransformationService, ShipweightTransformationService>();
        services.AddScoped<IFusekiService, FusekiService>();
        services.AddScoped<IMelDomReaderService, MelDomReaderService>();
        services.AddScoped<IProvenanceService, ProvenanceService>();
        services.AddScoped<IRdfGraphService, RdfGraphService>();
        services.AddScoped<IRdfPreprocessingService, RdfPreprocessingService>();
        services.AddScoped<IRdfService, RdfService>();
        services.AddScoped<IXmlRdfService, XmlRdfService>();
        services.AddScoped<IRdfTableBuilderFactory, RdfTableBuilderFactory>();
        services.AddScoped<IRdfTableBuilderService, MelRdfTableBuilderService>();
        services.AddScoped<IRdfTableBuilderService, ShipweightRdfTableBuilderService>();
        services.AddScoped<IRdfTransformationService, RdfTransformationService>();
        services.AddScoped<ISpreadsheetTransformationService, LineListTransformationService>();
        services.AddScoped<ISpreadsheetTransformationService, MelTransformationService>();
        services.AddScoped<IXMLTransformationService, AmlTransformationService>();
        services.AddScoped<AmlToRdfConverter>();
        services.AddScoped<ITieMessageService, TieMessageService>();
        return services;
    }
}