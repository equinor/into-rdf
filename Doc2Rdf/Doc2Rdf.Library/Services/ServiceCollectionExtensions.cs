using Microsoft.Extensions.DependencyInjection;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.IO;
using Doc2Rdf.Library.Services;

namespace Doc2Rdf.Library.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDoc2RdfLibraryServices(
        this IServiceCollection services)
    {
        services.AddTransient<IMelReader, DomMelReader>();
        services.AddTransient<IRdfTransformer, RdfTransformer>();
        services.AddTransient<IMelTransformer, MelTransformer>();
        services.AddTransient<IShipWeightTransformer, ShipWeightTransformer>();
        services.AddTransient<IRdfPreprocessor, RdfPreprocessor>();
        services.AddTransient<IRdfTableBuilderFactory, RdfTableBuilderFactory>();
        services.AddTransient<RdfMelTableBuilder>()
                        .AddTransient<IRdfTableBuilder, RdfMelTableBuilder>(s => s.GetService<RdfMelTableBuilder>());
        services.AddTransient<RdfShipWeightTableBuilder>()
                        .AddTransient<IRdfTableBuilder, RdfShipWeightTableBuilder>(s => s.GetService<RdfShipWeightTableBuilder>());
        services.AddTransient<IRdfGraphWrapper, RdfGraphWrapper>();
        return services;
    }
}
