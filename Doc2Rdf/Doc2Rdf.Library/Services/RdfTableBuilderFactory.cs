using System;
using System.Collections.Generic;
using System.Linq;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Doc2Rdf.Library.Services;

public class RdfTableBuilderFactory : IRdfTableBuilderFactory
{
    private readonly IEnumerable<IRdfTableBuilder> _rdfTableBuilders;

    public RdfTableBuilderFactory(IEnumerable<IRdfTableBuilder> rdfTableBuilders)
    {
        _rdfTableBuilders = rdfTableBuilders;
    }

    public IRdfTableBuilder GetRdfTableBuilder(string dataSource)
    {
        var builder = _rdfTableBuilders.FirstOrDefault(x => x.GetBuilderType() == dataSource) ?? throw new ArgumentException($"Builder of type {dataSource} not available");

        return builder;
    }
}
