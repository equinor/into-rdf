namespace Common.TieModels;

public class TieData
{
    public TieFileData FileData { get; }

    public TieInterfaceData InterfaceData { get; }

    public TieObjectData ObjectData { get; }

    public TieData(TieFileData fileData, TieInterfaceData interfaceData, TieObjectData objectData)
    {
        FileData = fileData;
        InterfaceData = interfaceData;
        ObjectData = objectData;
    }
}