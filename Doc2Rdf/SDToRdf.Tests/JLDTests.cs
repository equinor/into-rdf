using System;
using Xunit;
using System.IO;
using SDToRdf.Lib;

namespace SD2Rdf.Tests
{
    public class JLDTests
    {
        [Fact]
        public void ContextExamples()
        {
            /*
             * @Context is HREF: This results in HTTP GET of URL in Context.Will drop properties not in the context
             * Properties annotated in @Context is a hard restriction on what properties will be converted.
             * Due to @Context being potentially elsewhere and the above restriction we get a strict schema for the contents
             * while the schema definition is elsewhere and potentially out of our control.
            */
            var JsonLDAsString = new JsonLdToRdf().MapFromFile("TestData/CallHome.json");
            //Embedded Context: This does not call home
            var EmbeddedContext = new JsonLdToRdf().MapFromFile("TestData/EmbeddedContext.json");
            //Expanded Notation: Does not call home
            var ExpandedNotation = new JsonLdToRdf().MapFromFile("TestData/Expanded.json");
            //Compact notation: Does not call home
            var CompactNotation = new JsonLdToRdf().MapFromFile("TestData/Compacted.json");
            //Shorthand @Context: Does not call home
            var Eiriks = new JsonLdToRdf().MapFromFile("TestData/EiriksAttributter.json");
        }
    }
}
