using RDFSharp.Model;
using System.Xml;
using System.Xml.Schema;
using Services.TransformationServices.XMLTransformationServices.Serializers;

namespace Services.TransformationServices.XMLTransformationServices.Converters;


public class AmlToRdfConverter
{
    private static readonly RDFResource amlBase = new RDFResource("https://rdf.equinor.com/drafts/aml/");
    private static readonly RDFResource amlDescription = new RDFResource(amlBase + "description");
    private static readonly RDFResource amlAttributeVale = new RDFResource(amlBase + "attributeValue");
    private static readonly RDFResource amlDefaultAttributeVale = new RDFResource(amlBase + "defaultAttributeValue");
    private static readonly RDFResource amlXmlNesting = new RDFResource(amlBase + "hasPart");
    private static readonly RDFResource amlHasAttribute = new RDFResource(amlBase + "hasAttribute");
    private static readonly RDFResource amlHasConstraint = new RDFResource(amlBase + "hasConstraint");
    private static readonly RDFResource amlHasExternalInterface = new RDFResource(amlBase + "hasExternalInterface");
    private static readonly RDFResource amlInternalElement = new RDFResource(amlBase + "InternalElement");
    private static readonly RDFResource amlInterfaceClass = new RDFResource(amlBase + "InterfaceClass");
    private static readonly RDFResource amlAttribute = new RDFResource(amlBase + "Attribute");
    private static readonly RDFResource amlConstraint = new RDFResource(amlBase + "Constraint");
    private static readonly RDFResource amlExternalInterface = new RDFResource(amlBase + "ExternalInterface");
    private static readonly RDFResource a = new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
    private static readonly RDFResource rdfslabel = new RDFResource("http://www.w3.org/2000/01/rdf-schema#label");

    public static string Convert(Stream amlStream)
    {

        var amlGraph = new RDFGraph();

        XmlReaderSettings settings = new XmlReaderSettings();
        ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
        var reader = XmlReader.Create(amlStream, settings);
        var aml = new XmlDocument();
        aml.Load(reader);
        aml.Schemas.Add("http://www.dke.de/CAEX", "https://dugtrioexperimental.blob.core.windows.net/validation-schemas/CAEX_ClassModel_V.3.0.xsd");
        aml.Validate(eventHandler);

        traverseXml(aml, amlGraph);

        //Made a custom extention to enable quick serialization to nquad given a context so we dont have to go via an in memorory triplestore in RdfSharp to generate NQuad.
        return amlGraph.TripleToNQuad();
    }
    static void ValidationEventHandler(object? sender, ValidationEventArgs args)
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

    private static void traverseXml(XmlNode node, RDFGraph amlGraph)
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

    private static void decomposeAttributeTypeLib(XmlElement node, RDFGraph amlGraph)
    {
        //Do nothing for now
    }

    private static void decomposeSystemUnitClassLib(XmlElement node, RDFGraph amlGraph)
    {
        //Do nothing for now
    }

    private static void decomposeRoleClassLib(XmlElement node, RDFGraph amlGraph)
    {
        //Do nothing for now
    }

    private static void decomposeInstanceHierachy(XmlElement xmlelement, RDFGraph amlGraph)
    {
        RDFResource? InstanceHierarchy = null;
        var xmlNameAttribute = xmlelement.GetAttribute("Name");
        if (xmlelement.Attributes is not null && xmlelement.HasChildNodes && xmlNameAttribute is not null)
        {
            var urlEncodedName = System.Web.HttpUtility.UrlEncode(xmlNameAttribute, System.Text.Encoding.UTF8);
            InstanceHierarchy = new RDFResource(amlBase + xmlelement.Name + "/" + urlEncodedName);
            amlGraph.AddTriple(new RDFTriple(InstanceHierarchy, a, new RDFResource(amlBase + xmlelement.Name)));
            var versionElem = xmlelement.GetElementsByTagName("Version").Item(0);
            if (versionElem is not null)
            {
                amlGraph.AddTriple(new RDFTriple(InstanceHierarchy, new RDFResource(amlBase + versionElem.Name), RDFStringLiteral(versionElem.InnerText)));
            }
        }
        if (InstanceHierarchy is not null)
        {
            foreach (XmlElement internalElement in xmlelement.GetElementsByTagName("InternalElement"))
            {
                if (!internalElement.Equals(xmlelement))
                {
                    decomposeInternalElement(InstanceHierarchy, internalElement, amlGraph);
                }
            }
        }

    }

    private static void decomposeInternalElement(RDFResource parent, XmlElement internalElement, RDFGraph amlGraph)
    {
        var internalElementRdf = new RDFResource(amlBase + internalElement.GetAttribute("ID"));

        amlGraph.AddTriple(new RDFTriple(parent, amlXmlNesting, internalElementRdf));
        if (internalElement.GetAttribute("RefBaseSystemUnitPath").StartsWith("DocumentClassLibrary/"))
        {
            var context = amlBase + internalElement.GetAttribute("Name");
            amlGraph.SetContext(new Uri(context));
        }
        var RefBaseSystemUnitPathRdf = new RDFResource(amlBase + internalElement.GetAttribute("RefBaseSystemUnitPath"));
        amlGraph.AddTriple(new RDFTriple(internalElementRdf, a, RefBaseSystemUnitPathRdf));
        amlGraph.AddTriple(new RDFTriple(internalElementRdf, a, amlInternalElement));
        amlGraph.AddTriple(new RDFTriple(parent, amlXmlNesting, internalElementRdf));

        amlGraph.AddTriple(new RDFTriple(internalElementRdf, rdfslabel, RDFStringLiteral(internalElement.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD. Cardinality is always 0 or 1.
        AddLiteralFromInnerText(internalElementRdf, amlDescription, internalElement.GetElementsByTagName("Description").Item(0), amlGraph);
        foreach (XmlElement nestedElement in internalElement.ChildNodes)
        {
            switch (nestedElement.Name)
            {
                case "InternalElement":
                    decomposeInternalElement(internalElementRdf, nestedElement, amlGraph);
                    break;
                case "Attribute":
                    decomposeAttribute(internalElementRdf, nestedElement, amlGraph);
                    break;
                case "InternalLink":
                    decomposeInternalLink(nestedElement, amlGraph);
                    break;
                case "ExternalInterface":
                    decomposeExternalInterface(internalElementRdf, nestedElement, amlGraph);
                    break;
                default: break;
            }
        }
    }

    private static void decomposeExternalInterface(RDFResource parent, XmlElement element, RDFGraph amlGraph)
    {
        var externalInterfaceRdf = new RDFResource(amlBase + element.GetAttribute("ID"));
        var RefBaseSystemUnitPathRdf = new RDFResource(amlBase + element.GetAttribute("RefBaseClassPath"));

        amlGraph.AddTriple(new RDFTriple(externalInterfaceRdf, a, RefBaseSystemUnitPathRdf));
        amlGraph.AddTriple(new RDFTriple(externalInterfaceRdf, a, amlExternalInterface));
        amlGraph.AddTriple(new RDFTriple(parent, amlHasExternalInterface, externalInterfaceRdf));

        amlGraph.AddTriple(new RDFTriple(externalInterfaceRdf, rdfslabel, RDFStringLiteral(element.GetAttribute("Name"))));
        //Fetchin Item(0) is reliable due to cardinality defined in XSD
        AddLiteralFromInnerText(externalInterfaceRdf, amlDescription , element.GetElementsByTagName("Description").Item(0), amlGraph);
        if (externalInterfaceRdf is not null)
        {
            foreach (XmlNode nestedElement in element.ChildNodes)
            {
                if (nestedElement.Name == "Attribute")
                {
                    decomposeAttribute(externalInterfaceRdf, (XmlElement) nestedElement, amlGraph);
                }
            }
        }
    }


    private static void decomposeInternalLink(XmlElement internalLink, RDFGraph amlGraph)
    {
        RDFResource RefPartnerSideA = new RDFResource(amlBase + internalLink.GetAttribute("RefPartnerSideA"));
        RDFResource RefPartnerSideB = new RDFResource(amlBase + internalLink.GetAttribute("RefPartnerSideB"));
        RDFResource Name = new RDFResource(amlBase + internalLink.GetAttribute("Name").Replace(" ", ""));
        amlGraph.AddTriple(new RDFTriple(RefPartnerSideA, Name, RefPartnerSideB));
        amlGraph.AddTriple(new RDFTriple(Name, a, new RDFResource(amlBase+"internalLink")));

    }

    private static void decomposeInterfaceClassLibrary(XmlElement xmlelement, RDFGraph amlGraph)
    {
        RDFResource? InterfaceClassLibrary = null;
        if (xmlelement.Attributes is not null && xmlelement.HasChildNodes)
        {
            InterfaceClassLibrary = new RDFResource(amlBase + xmlelement.Name + xmlelement.GetAttribute("Name"));
            amlGraph.AddTriple(new RDFTriple(InterfaceClassLibrary, a, new RDFResource(amlBase + xmlelement.Name)));
            var versionElem = xmlelement.GetElementsByTagName("Version").Item(0);
            if (versionElem is not null)
            {
                amlGraph.AddTriple(new RDFTriple(InterfaceClassLibrary, new RDFResource(amlBase + versionElem.Name), RDFStringLiteral(versionElem.InnerText)));
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

    private static void decomposeInterfaceClass(RDFResource interfaceClassLibrary, XmlElement interfaceClass, RDFGraph amlGraph)
    {
        RDFResource interfaceClassRdf;
        var id = interfaceClass.GetAttribute("ID");
        if (id is not null && id.Length > 0)
        {
            interfaceClassRdf = new RDFResource(amlBase + interfaceClass.GetAttribute("ID"));
        }
        else
        {
            interfaceClassRdf = new RDFResource(amlBase + interfaceClass.GetAttribute("Name"));
        }

        amlGraph.AddTriple(new RDFTriple(interfaceClassLibrary, amlXmlNesting, interfaceClassRdf));
        var RefBaseSystemUnitPathRdf = new RDFResource(amlBase + interfaceClass.GetAttribute("RefBaseClassPath"));
        if (RefBaseSystemUnitPathRdf.URI != new Uri("https://rdf.equinor.com/drafts/aml/"))
        {
            amlGraph.AddTriple(new RDFTriple(interfaceClassRdf, a, RefBaseSystemUnitPathRdf));
        }
        amlGraph.AddTriple(new RDFTriple(interfaceClassRdf, a, amlInterfaceClass));
        amlGraph.AddTriple(new RDFTriple(interfaceClassLibrary, amlXmlNesting, interfaceClassRdf));

        amlGraph.AddTriple(new RDFTriple(interfaceClassRdf, rdfslabel, RDFStringLiteral(interfaceClass.GetAttribute("Name"))));
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
    private static void decomposeAttribute(RDFResource parent, XmlElement attributeElement, RDFGraph amlGraph)
    {
        var attributeRdf = new RDFResource(parent.URI + "/" + attributeElement.GetAttribute("Name"));
        amlGraph.AddTriple(new RDFTriple(parent, amlHasAttribute, attributeRdf));
        amlGraph.AddTriple(new RDFTriple(attributeRdf, a, new RDFResource(amlAttribute.URI + "/" + attributeElement.GetAttribute("Name"))));
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

    private static void decomposeConstraint(RDFResource parent, XmlElement constraintElement, RDFGraph amlGraph)
    {
        var ConstraintRdf = new RDFResource(parent.URI + "/" + constraintElement.GetAttribute("Name"));
        amlGraph.AddTriple(new RDFTriple(parent, amlHasConstraint, ConstraintRdf));
        amlGraph.AddTriple(new RDFTriple(ConstraintRdf, a, amlConstraint));

        foreach (XmlElement constraintType in constraintElement.ChildNodes)
        {
            foreach (XmlElement constraintValueDef in constraintType.ChildNodes)
            {
                amlGraph.AddTriple(new RDFTriple(ConstraintRdf, new RDFResource(amlBase + constraintType.Name), RDFStringLiteral(constraintValueDef.InnerText)));
            }
        }
    }

    private static void AddLiteralFromInnerText(RDFResource subject, RDFResource predicate, XmlNode? node, RDFGraph amlGraph)
    {
        if (node is not null)
        {
            amlGraph.AddTriple(new RDFTriple(subject, predicate, RDFStringLiteral(node.InnerText)));
        }
    }

    private static RDFTypedLiteral RDFStringLiteral(string value)
    {
        value = System.Text.RegularExpressions.Regex.Replace(value, @"\r\n?|\n", " ");
        return new RDFTypedLiteral(value, RDFModelEnums.RDFDatatypes.XSD_STRING);
    }
}
