using VDS.RDF;
using System.Xml;
using System.Xml.Schema;
using VDS.RDF.Parsing;

namespace IntoRdf.TransformationServices.XMLTransformationServices.Converters;

internal class AmlToRdfConverter
{
    internal AmlToRdfConverter(Uri baseUri, List<(string, Uri)> identityCollectionsAndPatternsArgs)
    {
        amlGraph = new Graph();
        amlGraph.NamespaceMap.AddNamespace("aml", baseUri);
        amlGraph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        amlGraph.NamespaceMap.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
        amlGraph.NamespaceMap.AddNamespace("rec", new Uri("https://rdf.equinor.com/ontology/record/"));
        amlDescription = amlGraph.CreateUriNode("aml:description");
        amlAttributeVale = amlGraph.CreateUriNode("aml:attributeValue");
        amlDefaultAttributeVale = amlGraph.CreateUriNode("aml:defaultAttributeValue");
        amlXmlNesting = amlGraph.CreateUriNode("aml:XmlNesting");
        amlHasAttribute = amlGraph.CreateUriNode("aml:hasAttribute");
        amlHasConstraint = amlGraph.CreateUriNode("aml:hasConstraint");
        amlHasExternalInterface = amlGraph.CreateUriNode("aml:hasExternalInterface");
        amlInternalElement = amlGraph.CreateUriNode("aml:InternalElement");
        amlInterfaceClass = amlGraph.CreateUriNode("aml:InterfaceClass");
        amlAttribute = amlGraph.CreateUriNode("aml:Attribute");
        amlConstraint = amlGraph.CreateUriNode("aml:Constraint");
        amlExternalInterface = amlGraph.CreateUriNode("aml:ExternalInterface");
        a = amlGraph.CreateUriNode("rdf:type");
        rdfslabel = amlGraph.CreateUriNode("rdfs:label");

        internalElementBasedCollections = new List<(string, bool)>();
        identityCollectionsAndPatterns = identityCollectionsAndPatternsArgs;

    }

    private readonly Graph amlGraph;
    private IUriNode amlDescription;
    private IUriNode amlAttributeVale;
    private IUriNode amlDefaultAttributeVale;
    private IUriNode amlXmlNesting;
    private IUriNode amlHasAttribute;
    private IUriNode amlHasConstraint;
    private IUriNode amlHasExternalInterface;
    private IUriNode amlInternalElement;
    private IUriNode amlInterfaceClass;
    private IUriNode amlAttribute;
    private IUriNode amlConstraint;
    private IUriNode amlExternalInterface;
    private IUriNode a;
    private IUriNode rdfslabel;
    private readonly List<(string Pattern, Uri Uri)> identityCollectionsAndPatterns;
    private readonly List<(String Collection, bool IRIOverride)> internalElementBasedCollections;
    internal Graph Convert(Stream amlStream)
    {
        XmlDocument aml = validateAndGenerateAmlDocument(amlStream);
        var caexFiles = aml.GetElementsByTagName("CAEXFile");
        foreach (XmlElement caexFile in caexFiles)
        {
            startTraversalofCaexFile(caexFile);
        }
        return amlGraph;
    }

    private void startTraversalofCaexFile(XmlNode node)
    {
        traverseXml(node);
    }
    private void traverseXml(XmlNode node)
    {
        List<XmlElement> instanceHierarchies = new List<XmlElement>();
        List<XmlElement> systemUnitClassLibs = new List<XmlElement>();
        string amlVersion = string.Empty;
        foreach (XmlNode childNode in node.ChildNodes)
        {
            switch (childNode.Name)
            {
                case "AdditionalInformation":
                    //This handles the exception where the only information is a single attribute designating the aml version
                    if (!childNode.HasChildNodes && childNode.Attributes != null && childNode.Attributes.Count == 1)
                    {
                        amlVersion = childNode.Attributes[0].Value;
                    }
                    else decomposeAdditionalInformation((XmlElement)childNode);
                    break;
                case "InstanceHierarchy":
                    instanceHierarchies.Add((XmlElement)childNode);
                    // decomposeInstanceHierachy((XmlElement)childNode);
                    break;
                case "InterfaceClassLib":
                    // decomposeInterfaceClassLibrary((XmlElement)childNode);
                    break;
                case "RoleClassLib":
                    // decomposeRoleClassLib((XmlElement)childNode);
                    break;
                case "SystemUnitClassLib":
                    systemUnitClassLibs.Add((XmlElement)childNode);
                    break;
                case "AttributeTypeLib":
                    // decomposeAttributeTypeLib((XmlElement)childNode);
                    break;
                default:
                    traverseXml(childNode);
                    break;
            }
        }

        //Ordering matters here
        if (systemUnitClassLibs.Count > 0 && instanceHierarchies.Count > 0)
        {
            systemUnitClassLibs.ForEach(sul => internalElementBasedCollections.AddRange(decomposeSystemUnitClassLib(sul)));

            instanceHierarchies.ForEach(ih => decomposeInstanceHierachy(ih));
        }
    }

    private void decomposeAdditionalInformation(XmlElement node)
    {
        //Do nothing for now
    }


    private void decomposeAttributeTypeLib(XmlElement node)
    {
        //Do nothing for now
    }

    private List<(string, bool)> decomposeSystemUnitClassLib(XmlElement node)
    {
        var systemUnitClassLibs = new List<(string, bool)>();
        foreach (XmlElement child in node.ChildNodes)
        {
            if (child.Name == "SystemUnitClass")
            {
                foreach (XmlElement grandChild in child.ChildNodes)
                {
                    if (grandChild.Name == "SupportedRoleClass" && grandChild.GetAttribute("RefRoleClassPath") == "AutomationMLBaseRoleClassLib/AutomationMLBaseRole/Structure")
                    {
                        var systemUnitPath = $"{node.GetAttribute("Name")}/{child.GetAttribute("Name")}";
                        if (identityCollectionsAndPatterns.Any(e => e.Pattern == systemUnitPath))
                        {
                            systemUnitClassLibs.Add(new(systemUnitPath, true));
                        }
                        systemUnitClassLibs.Add(new(systemUnitPath, false));
                        break; //Found what we're looking for.
                    }
                }
            }
        }
        return systemUnitClassLibs;
    }
    private void decomposeInstanceHierachy(XmlElement xmlelement)
    {
        var xmlNameAttribute = xmlelement.GetAttribute("Name");
        if (xmlelement.Attributes is not null && xmlelement.HasChildNodes && xmlNameAttribute != string.Empty)
        {
            var urlEncodedName = System.Web.HttpUtility.UrlEncode(xmlNameAttribute, System.Text.Encoding.UTF8);
            IUriNode InstanceHierarchy = amlGraph.CreateUriNode("aml:" + urlEncodedName);
            amlGraph.Assert(new Triple(InstanceHierarchy, a, amlGraph.CreateUriNode("aml:" + xmlelement.Name)));
            if (InstanceHierarchy is not null)
            {
                foreach (XmlElement childNode in xmlelement.ChildNodes)
                {
                    if (isCollectionElement(childNode))
                    {
                        decomposeInternalElementTypedCollection(InstanceHierarchy, childNode);
                    }
                    else
                    {
                        AddIfBasicTextElement(childNode, InstanceHierarchy);
                    }
                }
            }
        }
    }

    private void decomposeInternalElementTypedCollection(INode parent, XmlElement focusedCollectionElement)
    {
        var focusedrdf = CreateAndAssertUriNode(parent, focusedCollectionElement);
        if (focusedrdf.NodeType != NodeType.Blank)
        {
            foreach (XmlElement child in focusedCollectionElement.ChildNodes)
            {
                if (isCollectionElement(child))
                {
                    decomposeInternalElementTypedCollection(focusedrdf, child);
                }
                else
                {
                    decomposeInternalElementInstance(parent, child);
                }
            }
            traverseXml(focusedCollectionElement);
        }
    }
    private void decomposeInternalElementInstance(INode parentCollection, XmlElement internalElement)
    {
        var internalElementRdf = CreateAndAssertUriNode(parentCollection, internalElement);
        if (internalElementRdf.NodeType != NodeType.Blank)
        {
            amlGraph.Assert(new Triple(internalElementRdf, rdfslabel, CreateLiteralNode(internalElement.GetAttribute("Name"))));
        }
        foreach (XmlElement nestedElement in internalElement.ChildNodes)
        {
            if (isCollectionElement(nestedElement))
            {
                decomposeInternalElementTypedCollection(internalElementRdf, nestedElement);
            }
            else
            {
                switch (nestedElement.Name)
                {
                    case "Attribute":
                        decomposeAttribute(internalElementRdf, (XmlElement)nestedElement);
                        break;
                    case "InternalLink":
                        decomposeInternalLink((XmlElement)nestedElement);
                        break;
                    case "ExternalInterface":
                        decomposeExternalInterface(internalElementRdf, (XmlElement)nestedElement);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void decomposeExternalInterface(INode parent, XmlElement element)
    {
        // var externalInterfaceRdf = amlGraph.CreateUriNode("aml:" +element.GetAttribute("ID"));
        //This is bad, but it enables the pattern where the InternalLink Elements referr to <parent.id>:<this.name> instead of this.ID.
        var externalInterfaceRdf = amlGraph.CreateUriNode(new Uri(parent + ":" + element.GetAttribute("Name")));

        var RefBaseSystemUnitPathRdf = amlGraph.CreateUriNode("aml:" + element.GetAttribute("RefBaseClassPath"));

        amlGraph.Assert(new Triple(parent, amlHasExternalInterface, externalInterfaceRdf));

        amlGraph.Assert(new Triple(externalInterfaceRdf, rdfslabel, CreateLiteralNode(element.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        foreach (XmlNode nestedElement in element.ChildNodes)
        {
            if (nestedElement.Name == "Attribute")
            {
                decomposeAttribute(externalInterfaceRdf, (XmlElement)nestedElement);
            }
        }
    }
    private void decomposeInternalLink(XmlElement internalLink)
    {
        IUriNode RefPartnerSideA = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("RefPartnerSideA"));
        IUriNode RefPartnerSideB = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("RefPartnerSideB"));
        IUriNode internalLinkNode = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("Name").Replace(" ", "").Split(".")[0]);
        amlGraph.Assert(new Triple(RefPartnerSideA, internalLinkNode, RefPartnerSideB));
        amlGraph.Assert(new Triple(internalLinkNode, a, amlGraph.CreateUriNode("aml:internalLink")));
    }
    private void decomposeAttribute(INode parent, XmlElement attributeElement)
    {
        var attributeRdf = amlGraph.CreateUriNode(new Uri(parent + "/" + attributeElement.GetAttribute("Name")));
        amlGraph.Assert(new Triple(parent, amlHasAttribute, attributeRdf));
        amlGraph.Assert(new Triple(attributeRdf, a, amlGraph.CreateUriNode(new Uri(amlAttribute.Uri + "/" + attributeElement.GetAttribute("Name")))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        AddLiteralFromInnerText(attributeRdf, amlDescription, attributeElement.GetElementsByTagName("Description").Item(0));
        AddLiteralFromInnerText(attributeRdf, amlAttributeVale, attributeElement.GetElementsByTagName("Value").Item(0));
        AddLiteralFromInnerText(attributeRdf, amlDefaultAttributeVale, attributeElement.GetElementsByTagName("DefaultValue").Item(0));

        foreach (XmlElement nestedElement in attributeElement.ChildNodes)
        {
            if (nestedElement.Name == "Constraint")
            {
                decomposeConstraint(attributeRdf, nestedElement);
            }
        }
    }
    private void decomposeConstraint(IUriNode parent, XmlElement constraintElement)
    {
        var ConstraintRdf = amlGraph.CreateUriNode(new Uri(parent.Uri + "/" + constraintElement.GetAttribute("Name")));
        amlGraph.Assert(new Triple(parent, amlHasConstraint, ConstraintRdf));
        amlGraph.Assert(new Triple(ConstraintRdf, a, amlConstraint));

        foreach (XmlElement constraintType in constraintElement.ChildNodes)
        {
            foreach (XmlElement constraintValueDef in constraintType.ChildNodes)
            {
                amlGraph.Assert(new Triple(ConstraintRdf, amlGraph.CreateUriNode("aml:" + constraintType.Name), CreateLiteralNode(constraintValueDef.InnerText)));
            }
        }
    }
    private bool AddIfBasicTextElement(XmlElement node, INode subject)
    {
        if (IsPrimitiveText(node))
        {
            AddLiteralFromInnerText(subject, amlGraph.CreateUriNode("aml:" + node.Name), node);
        }
        return IsPrimitiveText(node);
    }

    private static bool IsPrimitiveText(XmlElement node)
    {
        if (!node.HasAttributes && node.ChildNodes.Count == 1)
        {
            var child = node.ChildNodes[0];
            if (child != null)
            {
                return child.Name == "#text";
            }
        }
        return false;
    }

    private void AddLiteralFromInnerText(INode subject, INode predicate, XmlNode? node)
    {
        if (node is not null)
        {
            amlGraph.Assert(new Triple(subject, predicate, CreateLiteralNode(node.InnerText)));
        }
    }
    private ILiteralNode CreateLiteralNode(string value)
    {
        value = System.Text.RegularExpressions.Regex.Replace(value, @"\r\n?|\n", " ");
        return amlGraph.CreateLiteralNode(value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
    }
    private XmlDocument validateAndGenerateAmlDocument(Stream amlStream)
    {
        XmlReaderSettings settings = new XmlReaderSettings();
        ValidationEventHandler eventHandler = new ValidationEventHandler(amlValidationHandler);
        var reader = XmlReader.Create(amlStream, settings);
        var aml = new XmlDocument();
        aml.Load(reader);
        using (FileStream fs = new FileStream("Schemas/CAEX_ClassModel_V.3.0.xsd", FileMode.Open))
        {
            var schemaReader = XmlReader.Create(fs);
            aml.Schemas.Add("http://www.dke.de/CAEX", schemaReader);
            aml.Validate(eventHandler);
        }
        return aml;
    }
    void amlValidationHandler(object? sender, ValidationEventArgs args)
    {
        switch (args.Severity)
        {
            case XmlSeverityType.Error:
                throw new Exception($"INVALID AML. Error: {args.Message}");
            case XmlSeverityType.Warning:
                break;
            default:
                break;
        }
    }

    private bool isCollectionElement(XmlElement focusedElement)
    {

        return internalElementBasedCollections.Any(e => e.Collection == focusedElement.GetAttribute("RefBaseSystemUnitPath"));
    }

    private INode CreateAndAssertUriNode(INode parent, XmlElement focusedXml)
    {
        return CreateAndAssertUriNodeFromNamedAttribute(parent, focusedXml, "Name");
    }

    //Returns blank node if unable to find named attribute value
    private INode CreateAndAssertUriNodeFromNamedAttribute(INode parent, XmlElement focusedXml, string namedAttribute)
    {
        var namedAttributeElement = focusedXml.Attributes.GetNamedItem(namedAttribute);
        if (namedAttributeElement is not null)
        {
            var focusedrdf = amlGraph.CreateUriNode("aml:" + namedAttributeElement.Value);
            amlGraph.Assert(parent, amlXmlNesting, focusedrdf);
            var typeAttribute = focusedXml.Attributes.GetNamedItem("RefBaseSystemUnitPath");
            if (typeAttribute is not null)
            {
                amlGraph.Assert(focusedrdf, a, amlGraph.CreateUriNode("aml:" + typeAttribute.Value));
            }
            var idAttribute = focusedXml.Attributes.GetNamedItem("ID");
            if (idAttribute is not null)
            {
                amlGraph.Assert(focusedrdf, amlGraph.CreateUriNode("aml:ID"), amlGraph.CreateUriNode("aml:" + idAttribute.Value));
            }
            return focusedrdf;
        }
        return amlGraph.CreateBlankNode();
    }
}
