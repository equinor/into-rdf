using System;
using Xunit;
using System.IO;
using SD2Rdf.Lib;
using System.Linq;
using System.Collections.Generic;

namespace SD2Rdf.Tests
{
    public class DexpiXML2RDFTests
    {
        [Fact]
        public void DexpiXML2RDF()
        {
            var files = Directory.GetFiles("TestData", "*.xml").ToList();
            var dexpiMapper = new DexpiXml2Rdf();
            var turtles = new List<string>();

            files.ForEach(f => turtles.Add(dexpiMapper.Map(f, File.Open(f, FileMode.Open))));

            Assert.True(turtles.Any()); //Silly autopilot autocomplete
        }
    }
}
