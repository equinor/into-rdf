using Doc2Rdf.Library.Models;
using System.IO;

namespace Doc2Rdf.Library.Interfaces;

public interface IMelTransformer
{
    string Transform(Stream excelStream, string fileName);
    string Transform(Stream excelStream, SpreadsheetDetails details);
}


