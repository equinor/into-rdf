using System;
using System.IO;
using Doc2Rdf.Common;
using Doc2Rdf.Common.Interfaces;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using VDS.RDF;

namespace SD2Rdf.Lib
{
    public class DexpiXml2Rdf : ITtlMapper
    {
        List<string> DexpiXMLElementExtractConfig;

        public const string EquinorDexpiPrefix = "eqdx";
        public const string DexpiPrefix = "dexpi";
        public readonly Uri EquinorUri = new Uri("http://rdf.equinor.com");


        public DexpiXml2Rdf()
        {
            DexpiXMLElementExtractConfig = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("DexpiExtractConfig.json"));

        }


        public string Map(string filename, Stream inputStream)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(inputStream);

            var nodesOfInterest = new List<INode>();

            var graph = new Graph();
            findNodesOfInterest(xmlDoc.ChildNodes, graph);


            throw new NotImplementedException();
        }


        private IGraph findNodesOfInterest(XmlNodeList xmlNodeList, IGraph graph)
        {
            var enumerator = xmlNodeList.GetEnumerator();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.ChildNodes.Count > 0)
                {
                    findNodesOfInterest(xmlNode.ChildNodes, graph);
                }
                if (DexpiXMLElementExtractConfig.Contains(xmlNode.Name))
                {
                    var IdNode = TriplesFromXmlNode(xmlNode, graph);
                    foreach (XmlNode genericAttributesNode in xmlNode.ChildNodes)
                    {
                        if (genericAttributesNode.Name == "GenericAttributes")
                        {
                            foreach (XmlNode genericAttribute in genericAttributesNode.ChildNodes)
                            {
                                TriplesFromXmlNode(genericAttribute, graph, IdNode);
                            }
                        }
                    }
                }
            }
            return graph;
        }
        private IUriNode TriplesFromXmlNode(XmlNode node, IGraph graph, IUriNode IDNode = null)
        {
            if (IDNode == null)
            {
                string ID = null;
                foreach (XmlAttribute xmlAttribute in node.Attributes)
                {
                    switch (xmlAttribute.Name)
                    {
                        case "ID":
                            ID = xmlAttribute.Value;
                            break;
                    }
                    if (ID != string.Empty)
                    {
                        IDNode = graph.CreateUriNode(new Uri(EquinorUri, $"{EquinorDexpiPrefix}#{ID}"));
                        graph.Assert(IDNode, graph.CreateUriNode(new Uri(EquinorUri, $"{DexpiPrefix}#PlantItem")), graph.CreateLiteralNode(node.Name));
                        break;
                    }
                }
            }
            foreach (XmlAttribute xmlAttribute in node.Attributes)
            {
                if (xmlAttribute.Name != "ID")
                {
                    graph.Assert(IDNode, graph.CreateUriNode(new Uri($"{EquinorUri}{EquinorDexpiPrefix}#{xmlAttribute.Name}")), graph.CreateLiteralNode(xmlAttribute.Value));
                }
            }

            return IDNode;
        }
    }
}
