using System;
using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;

namespace Doc2Rdf.Library
{
    public class RdfTableBuilderFactory
    {
        DataSource _dataSource;
        public RdfTableBuilderFactory(DataSource dataSource)
        {
            _dataSource = dataSource;
        }
        internal IRdfTableBuilder GetRdfTableBuilder(string tableName)
        {
            switch (_dataSource) 
            {
                case DataSource.Mel:
                    return new RdfMelTableBuilder(tableName);
                case DataSource.Shipweight:
                    return new RdfShipWeightTableBuilder(tableName);
                default:
                    throw new InvalidOperationException($"Unknown source: {_dataSource}");
            }
        }
    }
}