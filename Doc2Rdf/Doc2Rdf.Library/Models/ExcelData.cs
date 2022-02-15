using System.Collections.Generic;

namespace Doc2Rdf.Library.Models
{
    internal class ExcelData
    {
        private List<List<string>> data;
        private List<string> header;

        public ExcelData(List<string> header, List<List<string>> data)
        {
            this.header = header;
            this.data = data;
        }

        public List<string> Header { get { return header; } }
        public List<List<string>> Data { get { return data; } }
    }
}