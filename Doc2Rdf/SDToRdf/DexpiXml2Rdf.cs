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
using System.Text;
using VDS.RDF.Writing;

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
            var graph = new Graph();
            findNodesOfInterest(xmlDoc.ChildNodes, graph);
            MemoryStream outputStream = new MemoryStream();
            graph.SaveToStream(new StreamWriter(outputStream, Encoding.UTF8), new CompressingTurtleWriter());
            return Encoding.UTF8.GetString(outputStream.ToArray());
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
                    var IdNode = AddTriplesFromXmlNodeToGraph(xmlNode, graph);
                    foreach (XmlNode genericAttributesNode in xmlNode.ChildNodes)
                    {
                        if (genericAttributesNode.Name == "GenericAttributes")
                        {
                            foreach (XmlNode genericAttribute in genericAttributesNode.ChildNodes)
                            {
                                AddTriplesFromGenericAttributesNodeToGraph(genericAttribute, graph, IdNode);
                            }
                        }
                    }
                }
            }
            return graph;
        }
        private IUriNode AddTriplesFromXmlNodeToGraph(XmlNode node, IGraph graph)
        {
            IUriNode IDNode = null;
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
        private void AddTriplesFromGenericAttributesNodeToGraph(XmlNode xmlNode, IGraph graph, IUriNode ParentNode)
        {
            bool valueFound = false;
            IUriNode localNode = null;
            string genericAttribName = string.Empty;
            List<(INode, INode)> rightHandNodeTuples = new List<(INode, INode)>();
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                if (xmlAttribute.Name == "Name")
                {
                    genericAttribName = xmlAttribute.Value;
                    var localNodeUri = new Uri($"{ParentNode.Uri.ToString()}/{xmlAttribute.Value}");
                    localNode = graph.CreateUriNode(localNodeUri);
                }
                else
                {
                    if (xmlAttribute.Name == "Value")
                    {
                        valueFound = true;
                    }
                    var attribNameUri = new Uri(EquinorUri, xmlAttribute.Name);
                    rightHandNodeTuples.Add((graph.CreateUriNode(attribNameUri), graph.CreateLiteralNode(xmlAttribute.Value)));
                }
            }
            graph.Assert(ParentNode,graph.CreateUriNode(new Uri(EquinorUri, $"{DexpiPrefix}/rels/genericAttribute/{genericAttribName}")),localNode);
            if (!valueFound)
            {
                rightHandNodeTuples.Add((graph.CreateUriNode(new Uri(EquinorUri, "Value")), graph.CreateBlankNode()));
            }
            foreach ((INode, INode) rightHandTuple in rightHandNodeTuples)
            {
                graph.Assert(localNode, rightHandTuple.Item1, rightHandTuple.Item2);
            }
        }

    }
}
