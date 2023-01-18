using VDS.RDF;
using System.Xml;
using System.Xml.Schema;
using VDS.RDF.Parsing;
using System.Linq;

namespace Services.TransformationServices.XMLTransformationServices.Converters;

public class AmlToRdfConverter
{
    public AmlToRdfConverter(Uri baseUri)
    {
        amlGraph = new Graph();
        amlGraph.NamespaceMap.AddNamespace("aml", baseUri);
        amlGraph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        amlGraph.NamespaceMap.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
        amlGraph.NamespaceMap.AddNamespace("rec", new Uri("https://rdf.equinor.com/ontology/record/"));
        amlDescription = amlGraph.CreateUriNode("aml:description");
        amlAttributeVale = amlGraph.CreateUriNode("aml:attributeValue");
        amlDefaultAttributeVale = amlGraph.CreateUriNode("aml:defaultAttributeValue");
        amlXmlNesting = amlGraph.CreateUriNode("aml:hasPart");
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

    public Graph Convert(Stream amlStream)
    {
        XmlReaderSettings settings = new XmlReaderSettings();
        ValidationEventHandler eventHandler = new ValidationEventHandler(amlValidationHandler);
        var reader = XmlReader.Create(amlStream, settings);
        var aml = new XmlDocument();
        aml.Load(reader);
        //Sometime in the future the location of the XSD should be not hardcoded in the sourcecode. 
        aml.Schemas.Add("http://www.dke.de/CAEX", "https://dugtrioexperimental.blob.core.windows.net/validation-schemas/CAEX_ClassModel_V.3.0.xsd");
        aml.Validate(eventHandler);

        traverseXml(aml, amlGraph);

        return amlGraph;
    }
    void amlValidationHandler(object? sender, ValidationEventArgs args)
    {
        switch (args.Severity)
        {
            case XmlSeverityType.Error:
                Console.WriteLine($"INVALID AML. Error: {args.Message}");
                break;
            case XmlSeverityType.Warning:
                Console.WriteLine($"INVALID AML. Warning: {args.Message}");
                break;
            default:
                break;
        }
    }

    private void traverseXml(XmlNode node, Graph amlGraph)
    {
        foreach (XmlNode childNode in node.ChildNodes)
        {
            switch (node.Name)
            {
                case "InstanceHierarchy":
                    decomposeInstanceHierachy((XmlElement)node, amlGraph);
                    break;
                case "InterfaceClassLib":
                    decomposeInterfaceClassLibrary((XmlElement)node, amlGraph);
                    break;
                case "RoleClassLib":
                    decomposeRoleClassLib((XmlElement)node, amlGraph);
                    break;
                case "SystemUnitClassLib":
                    decomposeSystemUnitClassLib((XmlElement)node, amlGraph);
                    break;
                case "AttributeTypeLib":
                    decomposeAttributeTypeLib((XmlElement)node, amlGraph);
                    break;
                default:
                    traverseXml(childNode, amlGraph);
                    break;
            }
        }
    }

    private void decomposeAttributeTypeLib(XmlElement node, Graph amlGraph)
    {
        //Do nothing for now
    }

    private void decomposeSystemUnitClassLib(XmlElement node, Graph amlGraph)
    {
        //Do nothing for now
    }

    private void decomposeRoleClassLib(XmlElement node, Graph amlGraph)
    {
        //Do nothing for now
        //Do something soon
    }

    private void decomposeInstanceHierachy(XmlElement xmlelement, Graph amlGraph)
    {
        IUriNode InstanceHierarchy;
        var xmlNameAttribute = xmlelement.GetAttribute("Name");
        if (xmlelement.Attributes is not null && xmlelement.HasChildNodes && xmlNameAttribute is not null)
        {

            var urlEncodedName = System.Web.HttpUtility.UrlEncode(xmlNameAttribute, System.Text.Encoding.UTF8);
            InstanceHierarchy = amlGraph.CreateUriNode("aml:" + xmlelement.Name + "/" + urlEncodedName);
            amlGraph.Assert(new Triple(InstanceHierarchy, a, amlGraph.CreateUriNode("aml:" + xmlelement.Name)));
            var versionElem = xmlelement.GetElementsByTagName("Version").Item(0); //Fix her og<ø
            if (versionElem is not null)
            {
                amlGraph.Assert(new Triple(InstanceHierarchy, amlGraph.CreateUriNode("aml:" + versionElem.Name), CreateLiteralNode(versionElem.InnerText)));
            }
            if (InstanceHierarchy is not null)
            {
                foreach (XmlElement internalElement in xmlelement.ChildNodes)
                {
                    if (!internalElement.Equals(xmlelement))
                    {
                        decomposeInternalElement(InstanceHierarchy, internalElement, amlGraph);
                    }
                }
            }
        }

    }

    private void decomposeInternalElement(INode parent, XmlElement internalElement, Graph amlGraph)
    {
        var internalElementRdf = amlGraph.CreateUriNode("aml:" + internalElement.GetAttribute("ID"));

        amlGraph.Assert(new Triple(parent, amlXmlNesting, internalElementRdf));
        var RefBaseSystemUnitPathNode = amlGraph.CreateUriNode("aml:" + internalElement.GetAttribute("RefBaseSystemUnitPath"));
        amlGraph.Assert(new Triple(internalElementRdf, a, RefBaseSystemUnitPathNode));
        amlGraph.Assert(new Triple(internalElementRdf, a, amlInternalElement));
        amlGraph.Assert(new Triple(parent, amlXmlNesting, internalElementRdf));

        amlGraph.Assert(new Triple(internalElementRdf, rdfslabel, CreateLiteralNode(internalElement.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD. Cardinality is always 0 or 1.
        AddLiteralFromInnerText(internalElementRdf, amlDescription, internalElement.GetElementsByTagName("Description").Item(0), amlGraph);
        foreach (XmlNode nestedElement in internalElement.ChildNodes)
        {
            switch (nestedElement.Name)
            {
                case "InternalElement":
                    decomposeInternalElement(internalElementRdf, (XmlElement)nestedElement, amlGraph);
                    break;
                case "Attribute":
                    decomposeAttribute(internalElementRdf, (XmlElement)nestedElement, amlGraph);
                    break;
                case "InternalLink":
                    decomposeInternalLink((XmlElement)nestedElement, amlGraph);
                    break;
                case "ExternalInterface":
                    decomposeExternalInterface(internalElementRdf,(XmlElement) nestedElement, amlGraph);
                    break;
                default: break;
            }
        }
    }

    private void decomposeExternalInterface(IUriNode parent, XmlElement element, Graph amlGraph)
    {
        // var externalInterfaceRdf = amlGraph.CreateUriNode("aml:" +element.GetAttribute("ID"));
        //This is bad. Enables the pattern where the InternalLink Elements referr to <parent.id>:<this.name> instead of this.ID.
        var externalInterfaceRdf = amlGraph.CreateUriNode(new Uri(parent + ":" + element.GetAttribute("Name")));

        var RefBaseSystemUnitPathRdf = amlGraph.CreateUriNode("aml:" + element.GetAttribute("RefBaseClassPath"));

        amlGraph.Assert(new Triple(externalInterfaceRdf, a, RefBaseSystemUnitPathRdf));
        amlGraph.Assert(new Triple(externalInterfaceRdf, a, amlExternalInterface));
        amlGraph.Assert(new Triple(parent, amlHasExternalInterface, externalInterfaceRdf));

        amlGraph.Assert(new Triple(externalInterfaceRdf, rdfslabel, CreateLiteralNode(element.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        AddLiteralFromInnerText(externalInterfaceRdf, amlDescription, element.GetElementsByTagName("Description").Item(0), amlGraph);
        if (externalInterfaceRdf is not null)
        {
            foreach (XmlNode nestedElement in element.ChildNodes)
            {
                if (nestedElement.Name == "Attribute")
                {
                    decomposeAttribute(externalInterfaceRdf, (XmlElement)nestedElement, amlGraph);
                }
            }
        }
    }
    private void decomposeInternalLink(XmlElement internalLink, Graph amlGraph)
    {
        IUriNode RefPartnerSideA = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("RefPartnerSideA"));
        IUriNode RefPartnerSideB = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("RefPartnerSideB"));
        IUriNode Name = amlGraph.CreateUriNode("aml:" + internalLink.GetAttribute("Name").Replace(" ", "").Split(".")[0]);
        amlGraph.Assert(new Triple(RefPartnerSideA, Name, RefPartnerSideB));
        amlGraph.Assert(new Triple(Name, a, amlGraph.CreateUriNode("aml:internalLink")));

    }

    private void decomposeInterfaceClassLibrary(XmlElement xmlelement, Graph amlGraph)
    {
        IUriNode? InterfaceClassLibrary = null;
        if (xmlelement.Attributes is not null && xmlelement.HasChildNodes)
        {
            InterfaceClassLibrary = amlGraph.CreateUriNode("aml:" + xmlelement.Name + xmlelement.GetAttribute("Name"));
            amlGraph.Assert(new Triple(InterfaceClassLibrary, a, amlGraph.CreateUriNode("aml:" + xmlelement.Name)));
            var versionElem = xmlelement.GetElementsByTagName("Version").Item(0);
            if (versionElem is not null)
            {
                amlGraph.Assert(new Triple(InterfaceClassLibrary, amlGraph.CreateUriNode("aml:" + versionElem.Name), CreateLiteralNode(versionElem.InnerText)));
            }
        }
        if (InterfaceClassLibrary is not null)
        {
            foreach (XmlElement interfaceClass in xmlelement.GetElementsByTagName("InterfaceClass"))
            {
                if (!interfaceClass.Equals(xmlelement))
                {
                    decomposeInterfaceClass(InterfaceClassLibrary, interfaceClass, amlGraph);
                }
            }
        }
    }

    private void decomposeInterfaceClass(IUriNode interfaceClassLibrary, XmlElement interfaceClass, Graph amlGraph)
    {
        IUriNode interfaceClassRdf;
        var id = interfaceClass.GetAttribute("ID");
        if (id is not null && id.Length > 0)
        {
            interfaceClassRdf = amlGraph.CreateUriNode("aml:" + interfaceClass.GetAttribute("ID"));
        }
        else
        {
            interfaceClassRdf = amlGraph.CreateUriNode("aml:" + interfaceClass.GetAttribute("Name"));
        }

        amlGraph.Assert(new Triple(interfaceClassLibrary, amlXmlNesting, interfaceClassRdf));
        var RefBaseSystemUnitPathRdf = amlGraph.CreateUriNode("aml:" + interfaceClass.GetAttribute("RefBaseClassPath"));
        if (RefBaseSystemUnitPathRdf.Uri != new Uri("https://rdf.equinor.com/drafts/aml/"))
        {
            amlGraph.Assert(new Triple(interfaceClassRdf, a, RefBaseSystemUnitPathRdf));
        }
        amlGraph.Assert(new Triple(interfaceClassRdf, a, amlInterfaceClass));
        amlGraph.Assert(new Triple(interfaceClassLibrary, amlXmlNesting, interfaceClassRdf));

        amlGraph.Assert(new Triple(interfaceClassRdf, rdfslabel, CreateLiteralNode(interfaceClass.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        AddLiteralFromInnerText(interfaceClassRdf, amlDescription, interfaceClass.GetElementsByTagName("Description").Item(0), amlGraph);
        if (interfaceClassRdf is not null)
        {
            foreach (XmlElement nestedElement in interfaceClass.ChildNodes)
            {
                if (nestedElement.Name == "InterfaceClass")
                {
                    decomposeInterfaceClass(interfaceClassRdf, nestedElement, amlGraph);
                }
                else if (nestedElement.Name == "Attribute")
                {
                    decomposeAttribute(interfaceClassRdf, nestedElement, amlGraph);
                }
                else if (nestedElement.Name == "InternalLink")
                {
                    decomposeInternalLink(nestedElement, amlGraph);
                }
                else if (nestedElement.Name == "ExternalInterface")
                {
                    decomposeExternalInterface(interfaceClassRdf, nestedElement, amlGraph);
                }
            }
        }
    }
    private void decomposeAttribute(IUriNode parent, XmlElement attributeElement, Graph amlGraph)
    {
        var attributeRdf = amlGraph.CreateUriNode(new Uri(parent + "/" + attributeElement.GetAttribute("Name")));
        amlGraph.Assert(new Triple(parent, amlHasAttribute, attributeRdf));
        amlGraph.Assert(new Triple(attributeRdf, a, amlGraph.CreateUriNode(new Uri(amlAttribute.Uri + "/" + attributeElement.GetAttribute("Name")))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        AddLiteralFromInnerText(attributeRdf, amlDescription, attributeElement.GetElementsByTagName("Description").Item(0), amlGraph);
        AddLiteralFromInnerText(attributeRdf, amlAttributeVale, attributeElement.GetElementsByTagName("Value").Item(0), amlGraph);
        AddLiteralFromInnerText(attributeRdf, amlDefaultAttributeVale, attributeElement.GetElementsByTagName("DefaultValue").Item(0), amlGraph);

        foreach (XmlElement nestedElement in attributeElement.ChildNodes)
        {
            if (nestedElement.Name == "Constraint")
            {
                decomposeConstraint(attributeRdf, nestedElement, amlGraph);
            }
        }
    }

    private void decomposeConstraint(IUriNode parent, XmlElement constraintElement, Graph amlGraph)
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

    private void AddLiteralFromInnerText(INode subject, INode predicate, XmlNode? node, Graph amlGraph)
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
}
