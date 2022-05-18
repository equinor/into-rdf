using System;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library.Services;

public interface IRdfTableBuilderFactory
{
    IRdfTableBuilder GetRdfTableBuilder(DataSource dataSource);
}

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

    public IRdfTableBuilder GetRdfTableBuilder(DataSource dataSource)
    {
        switch (dataSource)
        {
            case DataSource.Mel:
                return (IRdfTableBuilder)_serviceProvider.GetService(typeof(RdfMelTableBuilder));
            case DataSource.Shipweight:
                return (IRdfTableBuilder)_serviceProvider.GetService(typeof(RdfShipWeightTableBuilder));
            default:
                throw new InvalidOperationException($"Unknown source: {dataSource}");
        }
    }
}
