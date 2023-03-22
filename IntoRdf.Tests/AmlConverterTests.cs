using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Writing;
using IntoRdf.Public.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IntoRdf.Tests
{
    public class AmlConverterTests
    {
        FileStream faultyAML = new FileStream("TestData/test.aml", FileMode.Open);
        [Fact]
        internal void AssertThrowsOnMalformedAML()
        {
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();
            var amlDetails = new AmlTransformationDetails(
                    new Uri("https://IAmTest/scd/"),
                new List<(string,Uri)>()
                    { ("IAmString" ,new Uri("https://iAmAlsoTest.com"))
                    }
                );
            Assert.Throws<System.Exception>(() => new TransformerService().TransformAml(amlDetails, faultyAML, RdfFormat.Turtle));
            
        }
    }
}