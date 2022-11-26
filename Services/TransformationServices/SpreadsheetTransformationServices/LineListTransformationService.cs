using Azure.Storage.Blobs.Models;
using Common.GraphModels;
using Common.ProvenanceModels;
using Common.SpreadsheetModels;
using Common.RevisionTrainModels;
using Microsoft.Extensions.Logging;
using Services.DomReaderServices.ExcelDomReaderServices;
using Services.TransformationServices.RdfTransformationServices;
using VDS.RDF;

namespace Services.TransformationServices.SpreadsheetTransformationServices;

 public class LineListTransformationService: ISpreadsheetTransformationService
 {
    private ILogger<LineListTransformationService> _logger;
    private string _dataSource;

    public LineListTransformationService(ILogger<LineListTransformationService> logger)
    {
        _logger = logger;
        _dataSource = DataSource.LineList;
    }

    public string Transform(RevisionTrainModel revisionTrain, Stream excelStream)
    {
        throw new NotImplementedException();
    }

    public string GetDataSource()
    {
        return _dataSource;
    } 


 }