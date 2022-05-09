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
        //Disabled as the tested method dexpiMapper.Map throws NotImplementedException
        [Fact]
        public void DexpiXML2RDF()
        {
            var files = Directory.GetFiles("TestData", "*.xml").ToList();
            var dexpiMapper = new DexpiXml2Rdf();
            var turtles = new List<(string, string)>();


            files.ForEach(f => turtles.Add((f, dexpiMapper.Map(f, File.Open(f, FileMode.Open)))));

            foreach((string, string) turtle in turtles )
            {
                File.WriteAllText($"{turtle.Item1}.ttl", turtle.Item2);
            }

            Assert.True(turtles.Any()); 
        }
    }
}
