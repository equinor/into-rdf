using System;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Doc2Rdf.Library.Services;

public class RdfTableBuilderFactory : IRdfTableBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RdfTableBuilderFactory(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException("No service provider added to RdfTableBuilder");
        }

        _serviceProvider = serviceProvider;
    }

    public IRdfTableBuilder GetRdfTableBuilder(string dataSource)
    {
        switch (dataSource)
        {
            case "mel":
                return (IRdfTableBuilder)_serviceProvider.GetRequiredService(typeof(RdfMelTableBuilder));
            case "shipweight":
                return (IRdfTableBuilder)_serviceProvider.GetRequiredService(typeof(RdfShipWeightTableBuilder));
            default:
                throw new InvalidOperationException($"Unknown source: {dataSource}");
        }
    }
}
