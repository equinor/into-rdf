using System.Data;
using IntoRdf.Public.Models;
using IntoRdf.TransformationServices;
using IntoRdf.Utils;
using Newtonsoft.Json;

internal class TabularJsonTransformationService : ITabularJsonTransformationService
{
    private readonly IRdfAssertionService _rdfAssertionService;
    public TabularJsonTransformationService(IRdfAssertionService rdfAssertionService)
    {
        _rdfAssertionService = rdfAssertionService;
    }
    public string TransformTabularJson(Stream content, RdfFormat outputFormat, string subjectProperty, TransformationDetails transformationDetails)
    {
        StreamReader reader = new StreamReader(content);
        string json = reader.ReadToEnd();
        if (json is null)
        {
            throw new ArgumentException("Result of reading content was null.");
        }
        DataTable? dt = JsonConvert.DeserializeObject<DataTable?>(json);
        if (dt is null)
        {
            throw new ArgumentException("Attempting to parse the JSON resulted in a null value");
        }
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
        var graph = _rdfAssertionService.AssertProcessedData(dt, $"id_{subjectProperty}");

        return GraphSupportFunctions.WriteGraphToString(graph, outputFormat);
    }
}