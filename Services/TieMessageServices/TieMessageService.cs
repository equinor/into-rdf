using Azure.Storage.Blobs.Models;
using Common.TieModels;
using System.Xml;

namespace Services.TieMessageServices;
public class TieMessageService : ITieMessageService
{
    public TieData ParseXmlMessage(List<BlobDownloadResult> blobData)
    {
        XmlDocument xmlDoc = new XmlDocument();

        BlobDownloadResult xmlBlob = GetXmlBlob(blobData);

        using Stream stream = xmlBlob.Content.ToStream();
        xmlDoc.Load(stream);
        TieFileData fileData = GetFileData(blobData);
        TieInterfaceData interfaceData = ParseInterfaceData(xmlDoc);
        TieObjectData objectData = ParseObjectData(xmlDoc);

        return new TieData(fileData, interfaceData, objectData);
    }

    private BlobDownloadResult GetXmlBlob(List<BlobDownloadResult> blobData)
    {
        return blobData
                .FirstOrDefault(blob => blob.Details.Metadata["Name"].ToLower().Contains("xml"))
                ?? throw new ArgumentException("xml file missing from blob data");
    }

    private TieFileData GetFileData(List<BlobDownloadResult> blobData)
    {
        var filename = blobData
            .FirstOrDefault(blob => blob.Details.Metadata["Name"].ToLower().Contains("xlsx"))
            ?.Details.Metadata["Name"] ?? throw new ArgumentException("xlsx file missing from blob data");

        return new TieFileData(filename);
    }

    private TieInterfaceData ParseInterfaceData(XmlDocument doc)
    {
        TieInterfaceData interfaceData = new TieInterfaceData();

        foreach (XmlNode documentChild in doc.ChildNodes)
        {
            if (documentChild.Name.Equals("InterfaceData"))
            {
                XmlAttributeCollection interfaceAttributes = documentChild.Attributes ??
                        throw new InvalidOperationException("Node 'InterfaceData' doesn't have attributes");

                foreach (XmlAttribute attr in interfaceAttributes)
                {
                    switch (attr.Name)
                    {
                        case "GUID":
                            interfaceData.Guid = attr.Value;
                            break;
                        case "PackageGuid":
                            interfaceData.PackageGuid = attr.Value;
                            break;
                        case "TimeStamp":
                            interfaceData.TimeStamp = attr.Value;
                            break;
                        case "Action":
                            interfaceData.Action = attr.Value;
                            break;
                        case "SourceSystem":
                            interfaceData.SourceSystem = attr.Value;
                            break;
                        case "Site":
                            interfaceData.Site = attr.Value;
                            break;
                        case "Project":
                            interfaceData.Project = attr.Value;
                            break;
                        case "ObjectName":
                            interfaceData.ObjectName = attr.Value;
                            break;
                        case "ObjectClass":
                            interfaceData.ObjectClass = attr.Value;
                            break;
                        case "ObjectType":
                            interfaceData.ObjectType = attr.Value;
                            break;
                        case "ObjectId":
                            interfaceData.ObjectId = attr.Value;
                            break;
                        case "MessageVersion":
                            interfaceData.MessageVersion = attr.Value;
                            break;
                        case "Classification":
                            interfaceData.Classification = attr.Value;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        return interfaceData;
    }

    private TieObjectData ParseObjectData(XmlDocument doc)
    {
        //Object data
        XmlNodeList nodes = doc.GetElementsByTagName("Object");
        XmlNode objectNode = nodes[0] ?? throw new InvalidOperationException("Object node doesn't exist in TIE xml message");
        XmlNodeList children = objectNode.ChildNodes;

        TieObjectData objectData = new TieObjectData();

        foreach (XmlNode child in children)
        {
            if (child.Name.Equals("Attributes"))
            {
                foreach (XmlNode attributeNode in child.ChildNodes)
                {
                    XmlAttributeCollection attributes = attributeNode.Attributes ??
                            throw new InvalidOperationException($"Node {attributeNode} doesn't have any attributes");

                    foreach (XmlAttribute attr in attributes)
                    {
                        if (attr.Name.Equals("Name"))
                        {
                            switch (attr.Value)
                            {
                                case "Name":
                                    objectData.Name = attributeNode.InnerText;
                                    break;
                                case "REV_STATUS":
                                    objectData.RevisionStatus = attributeNode.InnerText;
                                    break;
                                case "PURPOSE_CODE":
                                    objectData.PurposeCode = attributeNode.InnerText;
                                    break;
                                case "CONTR_CODE":
                                    objectData.ContractorCode = attributeNode.InnerText;
                                    break;
                                case "DISCIPLINE_CODE":
                                    objectData.DisciplineCode = attributeNode.InnerText;
                                    break;
                                case "DOC_TYPE":
                                    objectData.DocumentType = attributeNode.InnerText;
                                    break;
                                case "REV_PURPOSE_CODE":
                                    objectData.RevisionPurposeCode = attributeNode.InnerText;
                                    break;
                                case "REV_NO":
                                    objectData.RevisionNumber = attributeNode.InnerText;
                                    break;
                                case "DOC_TITLE":
                                    objectData.DocumentTitle = attributeNode.InnerText;
                                    break;
                                case "INST_CODE":
                                    objectData.InstallationCode = attributeNode.InnerText;
                                    break;
                                case "ORIGINATOR":
                                    objectData.Originator = attributeNode.InnerText;
                                    break;
                                case "PROJECT_CODE":
                                    objectData.ProjectCode = attributeNode.InnerText;
                                    break;
                                case "CONTRACT_NO":
                                    objectData.ContractNumber = attributeNode.InnerText;
                                    break;
                                case "REV_DATE":
                                    objectData.RevisionDate = attributeNode.InnerText;
                                    break;
                                case "DOC_NO":
                                    objectData.DocumentNumber = attributeNode.InnerText;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        return objectData;
    }
}