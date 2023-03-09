using System.Data;
using IntoRdf.Public.Models;
using IntoRdf.TransformationServices.RdfGraphServices;
using IntoRdf.Utils;
using Newtonsoft.Json;

internal class TabularJsonTransformationService : ITabularJsonTransformationService
{
    private readonly IRdfGraphService _rdfGraphService;
    public TabularJsonTransformationService(IRdfGraphService rdfGraphService)
    {
        _rdfGraphService = rdfGraphService;
    }
    public string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails)
    {
        StreamReader reader = new StreamReader(content);
        string json = reader.ReadToEnd();
        if (json is null)
        {
            throw new ArgumentException("Result of reading content was null.");
        }
        DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
        if (dt is not null)
        {
            var identityColumn = new DataColumn($"id_{subjectProperty}", typeof(Uri));
            identityColumn.ColumnName = $"id_{subjectProperty}";
            dt.Columns.Add(identityColumn);
            foreach (DataRow dr in dt.Rows)
            {
                dr[$"id_{subjectProperty}"] = new Uri($"{transformationDetails.BaseUri}{dr[subjectProperty]}");
            }
            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName != $"id_{subjectProperty}")
                {
                    dc.ColumnName = new Uri($"{transformationDetails.SourcePredicateBaseUri}{dc.ColumnName}").ToString();
                }
            }
            _rdfGraphService.AssertDataTable(dt, $"id_{subjectProperty}");
        }
        return GraphSupportFunctions.WriteGraphToString(_rdfGraphService.GetGraph(), outputFormat);
    }
}