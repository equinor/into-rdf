using System.Reflection;
using Common.TransformationModels;
using System.Text.Json;

namespace Api.Utils.Bindings;
public class TransformationBinding
{
    public IFormFile? File { get; set; }
    public SpreadsheetTransformationDetails? Details { get; set; }

    public static ValueTask<TransformationBinding?> BindAsync(HttpContext context,
                                               ParameterInfo parameter)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var file = context.Request.Form.Files["File"];
        var details = context.Request.Form["Details"].First();

        var transformationDetails = details != null 
            ? JsonSerializer.Deserialize<SpreadsheetTransformationDetails>(details, serializeOptions)
            : throw new InvalidOperationException("Missing transformation details");
        return ValueTask.FromResult<TransformationBinding?>(new TransformationBinding
        {
            File = file,
            Details = transformationDetails
        });
    }
}