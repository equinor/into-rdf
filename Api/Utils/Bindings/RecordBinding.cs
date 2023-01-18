namespace Api.Utils.Bindings;

public class RecordBinding
{
    public IFormFile? Record {get; set;}
    public int Cursor {get; set;}

    public static ValueTask<RecordBinding?> BindAsync(HttpContext context)
    {
        var record = context.Request.Form.Files["record"];
        var cursor = context.Request.Form["cursor"].First();

        return ValueTask.FromResult<RecordBinding?>(new RecordBinding
        {
            Record = record,
            Cursor = Int32.Parse(cursor)
        });
    }
}