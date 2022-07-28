using Common.ProvenanceModels;

namespace Common.TieModels;

public class TieData
{
    private TieFileData _fileData;
    private TieInterfaceData _interfaceData;
    private TieObjectData _objectData;

    public TieData(TieFileData fileData, TieInterfaceData interfaceData, TieObjectData objectData)
    {
        _fileData = fileData;
        _interfaceData = interfaceData;
        _objectData = objectData;
    }

    public RevisionRequirement GetRevisionRequirements()
    {
        return new RevisionRequirement(GetDocumentProjectId(), GetRevisionName(), GetRevisionDate());
    }

    public string GetDataCollectionName()
    {
        return _fileData.Name;
    }

    public string GetContractor()
    {
        return _objectData.ContractorCode ?? "NA";
    }

    private string GetDocumentProjectId()
    {
        var objectName = _interfaceData.ObjectName ?? throw new ArgumentException("Tie data doesn't contain document project id");
        var documentProjectId = _interfaceData.ObjectName[.._interfaceData.ObjectName.IndexOf("-", StringComparison.InvariantCulture)];

        return documentProjectId;
    }

    private string GetRevisionName()
    {
        return _objectData.RevisionNumber ?? throw new ArgumentException("Tie data doesn't contain revision number");
    }

    private DateTime GetRevisionDate()
    {
        DateTime result;
        bool success = DateTime.TryParse(_objectData.RevisionDate, out result);

        if (!success)
        {
            throw new ArgumentException("Failed to convert revision date to DateTime");
        }
        return result;
    }
}