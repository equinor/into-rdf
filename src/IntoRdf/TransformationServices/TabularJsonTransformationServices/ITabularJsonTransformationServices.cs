using IntoRdf.Models;

public interface ITabularJsonTransformationService
{
    string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails);
}