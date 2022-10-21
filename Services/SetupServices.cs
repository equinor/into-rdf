using Microsoft.Extensions.DependencyInjection;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.FusekiServices;
using Services.OntologyServices.OntologyService;
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

namespace Services.DependencyInjection;
public static class SetupServices
{
    public static IServiceCollection AddSplinterServices(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseTransformationService, ShipweightTransformationService>();
        services.AddScoped<IFusekiService, FusekiService>();
        services.AddScoped<IExcelDomReaderService, ExcelDomReaderService>();
        services.AddScoped<IProvenanceService, ProvenanceService>();
        services.AddScoped<IRdfGraphService, RdfGraphService>();
        services.AddScoped<IRdfPreprocessingService, RdfPreprocessingService>();
        services.AddScoped<IRdfService, RdfService>();
        services.AddScoped<IXmlRdfService, XmlRdfService>();
        services.AddScoped<IOntologyService, OntologyService>();
        services.AddScoped<IRdfTableBuilderFactory, RdfTableBuilderFactory>();
        services.AddScoped<IRdfTableBuilderService, ExcelRdfTableBuilderService>();
        services.AddScoped<IRdfTableBuilderService, ShipweightRdfTableBuilderService>();
        services.AddScoped<IRdfTableBuilderService, CommonLibTableBuilderService>();
        services.AddScoped<IRdfTransformationService, RdfTransformationService>();
        services.AddScoped<ISpreadsheetTransformationService, LineListTransformationService>();
        services.AddScoped<ISpreadsheetTransformationService, MelTransformationService>();
        services.AddScoped<ISourceToOntologyConversionService, SourceToOntologyConversionService>();
        services.AddScoped<IXMLTransformationService, AmlTransformationService>();
        services.AddScoped<AmlToRdfConverter>();
        services.AddScoped<ITieMessageService, TieMessageService>();
        services.AddScoped<ICommonLibService, CommonlibService>();
        services.AddScoped<ICommonLibTransformationService, CommonLibTransformationService>();
        services.AddScoped<ICommonLibToRdfService, CommonLibToRdfService>();
        
        
        return services;
    }

}