using Doc2Rdf.Library.Interfaces;
using Doc2Rdf.Library.Models;
using System;
using System.Data;
using System.IO;
using Doc2Rdf.Library.IO;

namespace Doc2Rdf.Library
{
    public class MelTransformer
    {
        public static string Transform(FileStream excelStream)
        {
            var melReader = new DomMelReader();
            var details = melReader.GetSpreadsheetDetails(excelStream);

            return Transform(excelStream, details);
        }

        public static string Transform(Stream excelStream, SpreadsheetDetails details)
        {
            var melReader = new DomMelReader();
            var data = melReader.GetSpreadsheetData(excelStream, details);

            DataSet dataset = new DataSet();
            dataset.Tables.Add(data);

            var provenance = CreateProvenance(details);

            var rdfTransformer = new RdfTransformer(); 
            return rdfTransformer.Transform(provenance, dataset);
        }

        private static Provenance CreateProvenance(SpreadsheetDetails details)
        {
            var facility = new FacilityIdentifiers(documentProjectId: details.ProjectCode);

            var previousRevision = details.Revision > 1 ? $"{details.ProjectCode}_{(details.Revision - 1).ToString("00")}" : string.Empty;

            var provenance = new Provenance(facility,
                                            details.FileName,
                                            details.Revision.ToString("00"),
                                            previousRevision,
                                            details.RevisionDate,
                                            DataSource.Mel,
                                            DataSourceType.File,
                                            DataFormat.Xlsx);

            return provenance;
        }
    }
}