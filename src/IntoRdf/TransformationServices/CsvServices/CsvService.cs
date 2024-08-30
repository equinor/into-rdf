using IntoRdf.Models;
using System.Data;
using VDS.RDF;

namespace IntoRdf.TransformationServices
{
    internal class CsvService : ICsvService
    {
        private readonly IDataTableProcessor _dataTableProcessor;
        private readonly IRdfAssertionService _rdfAssertionService;

        public CsvService(IDataTableProcessor dataTableProcessor, IRdfAssertionService rdfAssertionService)
        {
            _dataTableProcessor = dataTableProcessor;
            _rdfAssertionService = rdfAssertionService;
        }
        public Graph ConvertToRdf(CsvDetails csvDetails, TransformationDetails transformationDetails, Stream content)
        {
            var streamReader = new StreamReader(content).ReadToEnd();
            var stringReader = new StringReader(streamReader);
            var header = stringReader.ReadLine()?.Split(csvDetails.FieldDelimiter);
            if (header == null)
            {
                throw new Exception();
            }
            List<List<string>> data = new List<List<string>>();
            string? line;
            while ((line = stringReader.ReadLine()) != null)
            {
                data.Add(line.Split(csvDetails.FieldDelimiter).ToList());
            }
            var rawData = CreateDataTable(header.ToList(), data);
            var processedData = _dataTableProcessor.ProcessDataTable(transformationDetails, rawData);
            return _rdfAssertionService.AssertProcessedData(processedData);
        }

        private static DataTable CreateDataTable(List<string> headers, List<List<string>> data)
        {
            var inputDataTable = new DataTable();
            inputDataTable.TableName = "InputData";

            foreach (var header in headers)
            {
                inputDataTable.Columns.Add(header);
            }

            for (int i = 0; i < data.Count; i++)
            {
                var row = inputDataTable.NewRow();

                var dataColumns = data[i].Count > headers.Count ? headers.Count : data[i].Count;
                for (int j = 0; j < dataColumns; j++)
                {
                    row[j] = data[i][j];
                }
                inputDataTable.Rows.Add(row);
            }

            return inputDataTable;
        }
    }
}
