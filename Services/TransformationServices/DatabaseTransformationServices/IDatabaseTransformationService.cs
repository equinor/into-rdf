using System.Data;

namespace Services.TransformationServices.DatabaseTransformationServices;

public interface IDatabaseTransformationService
{
    string Transform(string facilityName, string plantId, DataTable inputData);
}