namespace Services.RecordServices;
public interface IRecordService
{
    Task Add(int cursor, Stream record, string contentType);
    Task Delete(Uri record);
}