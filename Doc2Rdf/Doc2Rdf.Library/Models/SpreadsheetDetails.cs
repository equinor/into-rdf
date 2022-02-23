using System;

namespace Doc2Rdf.Library.Models
{
    public class SpreadsheetDetails
    {
        public string ProjectCode { get; set; }
        public int Revision { get; set; }
        public bool IsTransposed { get; set; }
        public DateTime RevisionDate { get; set; }
        public string Contractor { get; set; }
        public DataSource DataSource { get; set; }
        public string SheetName { get; set; }
        public int HeaderRow { get; set; }
        public int DataStartRow { get; set; }
        public int DataEndRow { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string FileName { get; set; }
    }
}
