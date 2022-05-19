using System.Data;

namespace Doc2Rdf.Library.Interfaces
{
    public interface IShipWeightTransformer
    {
        string Transform(string facilityName, DataSet inputData);
    }
}