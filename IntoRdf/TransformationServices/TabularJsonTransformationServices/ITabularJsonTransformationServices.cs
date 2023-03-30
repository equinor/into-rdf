using IntoRdf.Public.Models;

public interface ITabularJsonTransformationService {
    string TransformTabularJson(Stream content, RdfFormat outputFormat, TransformationDetails transformationDetails);
}