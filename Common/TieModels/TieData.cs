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
        return new RevisionRequirement(GetFacilityId(), GetDocumentNumber(), GetRevisionName(), GetRevisionDate());
    }

    public string GetDataCollectionName()
    {
        return _fileData.Name;
    }

    public string GetContractor()
    {
        return _objectData.ContractorCode == String.Empty ? _objectData.ContractorCode : "NA";
    }

    public string GetDocumentProjectId()
    {
        var documentProjectId = !String.IsNullOrEmpty(_interfaceData.ObjectName) ?
            _interfaceData.ObjectName[.._interfaceData.ObjectName.IndexOf("-", StringComparison.InvariantCulture)] :
            String.Empty;

        return documentProjectId;
    }

    public string GetDocumentTitle()
    {
        return _objectData.DocumentTitle;
    }

    public string GetContractNumber()
    {
        return _objectData.ContractNumber;
    }

    public string GetProjectNumber()
    {
        return _objectData.ProjectCode;
    }

    private string GetFacilityId()
    {
        if (_interfaceData.Site != String.Empty)
        {
            return _interfaceData.Site;
        }

        if (_objectData.InstallationCode != String.Empty)
        {
            return _objectData.InstallationCode;
        }

        throw new InvalidOperationException(ErrorMessage(nameof(_objectData.InstallationCode)));
    }

    private string GetDocumentNumber()
    {
        if (!String.IsNullOrEmpty(_interfaceData.ObjectName))
        {
            return _interfaceData.ObjectName;
        }

        if (!String.IsNullOrEmpty(_objectData.DocumentNumber))
        {
            return _objectData.DocumentNumber;
        }

        throw new InvalidOperationException(ErrorMessage(nameof(_objectData.DocumentNumber)));
    }

    private string GetRevisionName()
    {
        return _objectData.RevisionNumber ?? throw new ArgumentException(ErrorMessage(nameof(_objectData.RevisionNumber)));
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

    private string ErrorMessage(string infoType)
    {
        return $"Tie data doesn't contain {infoType}";
    }
}