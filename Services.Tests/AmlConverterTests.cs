
using Common.ProvenanceModels;
using Services.TransformationServices.SpreadsheetTransformationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;
using Services.TransformationServices.XMLTransformationServices.Converters;

namespace Services.Tests
{
    public class AmlConverterTests
    {
        FileStream fs = new FileStream("TestData/test.aml", FileMode.Open);
        [Fact]
        public void AssertQuadsFromAML()
        {
            // Given
            var quads = AmlToRdfConverter.Convert(fs);
            // When
            var ts = new VDS.RDF.TripleStore();
            
            // Then
            ts.LoadFromString(quads);
            Assert.True(ts.Graphs.Count == 1);
            Assert.True(ts.Triples.Count() > 0);
            Assert.True(ts.Triples.All( t => t.Graph == ts.Graphs.First()));
        }
        
    }
}
