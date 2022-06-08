namespace Common.ProvenanceModels;

public static class DataSourceType
{
    public static string File() => "file";
    public static string Database() => "database";
    public static string Api() => "api";
    public static string Unknown() => "unknown";
}