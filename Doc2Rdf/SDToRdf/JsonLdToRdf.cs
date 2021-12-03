using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF;
using Doc2Rdf.Common.Interfaces;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using System.Linq;

namespace SDToRdf.Lib
{
    public class JsonLdToRdf
    {
        public string MapFromFile(string inputFile)
        {

            var store = new TripleStore();
            var parser = new JsonLdParser();

            parser.Load(store, inputFile);

            var outputStream = new MemoryStream();
            foreach(Graph g in store.Graphs)
            {
                g.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
            }
            return Encoding.UTF8.GetString(outputStream.ToArray());

        }
    }
}
