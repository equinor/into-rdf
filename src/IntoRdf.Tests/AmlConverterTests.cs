using System;
using System.IO;
using Xunit;
using System.Collections.Generic;
using IntoRdf.Models;
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
                new List<(string, Uri)>()
                    { ("IAmString" ,new Uri("https://iAmAlsoTest.com"))
                    }
                );
            Assert.Throws<System.Exception>(() => new TransformerService().TransformAml(amlDetails, faultyAML, RdfFormat.Turtle));

        }
    }
}