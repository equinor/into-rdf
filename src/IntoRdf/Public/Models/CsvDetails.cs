namespace IntoRdf.Models
{
    public class CsvDetails
    {
        public CsvDetails(string fieldDelimiter = ",", string rowDelimiter = "\n")
        {
            FieldDelimiter = fieldDelimiter;
            RowDelimiter = rowDelimiter;
        }

        public string FieldDelimiter { get; }
        public string RowDelimiter { get; }
    }
}
