using System.Reflection;

namespace Api.Utils.Bindings;
public class FileBinding
{
    public IFormFile? File { get; set; }

        public static ValueTask<FileBinding?> BindAsync(HttpContext context,
                                                   ParameterInfo parameter)
    {
        var file = context.Request.Form.Files["File"];
        return ValueTask.FromResult<FileBinding?>(new FileBinding
        {
            File = file
        });
    }
}