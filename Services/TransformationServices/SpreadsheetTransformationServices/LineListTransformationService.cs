using Azure.Storage.Blobs.Models;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

 public class LineListTransformationService: ISpreadsheetTransformationService
 {
    private string _dataSource;

    public LineListTransformationService()
    {
        _dataSource = DataSource.LineList();
    }

    public string Transform(Provenance provenance, BlobDownloadResult blob)
    {
        throw new NotImplementedException();
    }

    public string Transform(Stream excelStream, string fileName)
    {
        throw new NotImplementedException();
    }

    public string Transform(Stream excelStream, SpreadsheetInfo details)
    {
        throw new NotImplementedException();
    }

    public string GetDataSource()
    {
        return _dataSource;
    } 
 }