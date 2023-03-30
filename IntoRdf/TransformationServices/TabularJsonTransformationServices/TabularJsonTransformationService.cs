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
    public string TransformTabularJson(Stream content, RdfFormat outputFormat, TransformationDetails transformationDetails)
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
        var pdt = ProcessDataTable(transformationDetails, dt);
        var graph = _rdfAssertionService.AssertProcessedData(pdt, "subject");
        return GraphSupportFunctions.WriteGraphToString(graph, outputFormat);
    }

    private static DataTable ProcessDataTable(TransformationDetails transformationDetails, DataTable dt)
    {
        var identityColumn = new DataColumn($"subject", typeof(Uri));
        identityColumn.ColumnName = $"subject";
        dt.Columns.Add(identityColumn);
        if (transformationDetails.IdentifierTargetPathSegment is not null)
        {
            foreach (DataRow dr in dt.Rows)
            {
                dr["subject"] = new Uri($"{transformationDetails.BaseUri}{dr[transformationDetails.IdentifierTargetPathSegment.Target]}");
            }
        }
        else
        {
            foreach (DataRow dr in dt.Rows)
            {
                dr["subject"] = new Uri($"{transformationDetails.BaseUri}{Guid.NewGuid()}");
            }
        }
        List<(string from, string to)> columnPairs = new List<(string, string)>();
        List<DataColumn> newCols = new List<DataColumn>();
        foreach (DataColumn dc in dt.Columns)
        {
            Console.WriteLine(dc.ColumnName);
            if (dc.ColumnName != "subject")
            {
                var columnPredicateName = new Uri($"{transformationDetails.SourcePredicateBaseUri}{dc.ColumnName}").ToString();
                if (transformationDetails.TargetPathSegments.Select(e => e.Target).Contains(dc.ColumnName))
                {
                    if (dc.DataType.IsArray)
                    {
                        var newColumn = new DataColumn(columnPredicateName, typeof(Uri[]));
                        newCols.Add(newColumn);
                        columnPairs.Add((dc.ColumnName, columnPredicateName));
                    }
                    else
                    {
                        var newColumn = new DataColumn(columnPredicateName, typeof(Uri));
                        newCols.Add(newColumn);
                        columnPairs.Add((dc.ColumnName, columnPredicateName));
                    }
                }
                else
                {
                    dc.ColumnName = columnPredicateName;
                }
            }
        }
        dt.Columns.AddRange(newCols.ToArray());
        foreach (DataRow dr in dt.Rows)
        {
            foreach ((string from, string to) in columnPairs)
            {
                var dc = dt.Columns[from];
                if (dc != null && !dc.DataType.IsArray)
                {
                    dr[to] = new Uri($"{transformationDetails.BaseUri}{dr[from]}");
                }
                else
                {
                    if (dr[from].GetType().IsArray)
                    {
                        List<Uri> uris = new List<Uri>();
                        foreach (object element in (Array) dr[from])
                        {
                            uris.Add(new Uri($"{transformationDetails.BaseUri}{element}"));
                        }
                        dr[to] = uris.ToArray();
                    }
                }
            }
        }
        foreach ((string from, _) in columnPairs)
        {
            dt.Columns.Remove(from);
        }
        return dt;
    }
}