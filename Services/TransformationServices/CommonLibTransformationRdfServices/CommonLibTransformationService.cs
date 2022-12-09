using Common.GraphModels;
using Common.ProvenanceModels;
using Services.TransformationServices.RdfTransformationServices;
using System.Data;
using VDS.RDF;

namespace Services.CommonLibToRdfServices;

public class CommonLibTransformationService : ICommonLibTransformationService
{
    private readonly IRdfTransformationService _rdfTransformationService;

    public CommonLibTransformationService(IRdfTransformationService rdfTransformationService)
    {
        _rdfTransformationService = rdfTransformationService;
    }

    public ResultGraph Transform(Provenance provenance, Graph ontology, List<Dictionary<string, object>> records)
    {
        if (!records.Any()) return new ResultGraph(string.Empty, string.Empty);

        var dataTable = new DataTable
        {
            TableName = "InputData",
        };

        var first = records.First();
        var properties = first.GetType().GetProperties();
        var commonlibKeys = first.Keys;
        var types = first.Keys.Select(key => first[key]?.GetType());

        foreach (var prop in commonlibKeys)
            dataTable.Columns.Add(prop, typeof(string));

        foreach (var record in records)
        {
            var row = dataTable.NewRow();
            foreach (var prop in commonlibKeys.Where(key => record[key] is not null)) row[prop] = record[prop];
            dataTable.Rows.Add(row);
        }

        var resultGraph = _rdfTransformationService.Transform(provenance, ontology, dataTable);

        return resultGraph;
    }
    
}
