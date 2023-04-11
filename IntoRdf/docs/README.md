# IntoRdf
IntoRdf is a NuGet package for transforming Excel (XLSX) files into RDF (Resource Description Framework) format. This package helps developers easily convert spreadsheet data into RDF triples using a simple and convenient API.

## Features
Transform Excel (XLSX) files into Turtle or JSON-LD RDF formats.
Easily define custom namespaces and predicates for RDF individuals.
Supports various spreadsheet configurations, such as different sheet names, header rows, and data rows/columns.

## Installation
Install the IntoRdf NuGet package using the following command:

```
dotnet install IntoRdf
```

## Usage
To use IntoRdf, import the following namespaces:

```csharp
using IntoRdf;
using IntoRdf.Models;
```

Create an instance of ITransformerService to transform spreadsheets into RDF:

```csharp
ITransformerService transformerService = new TransformerService();
```

Then, create SpreadsheetDetails and TransformationDetails objects to define the input spreadsheet structure and the desired RDF output:

```csharp
var spreadsheetDetails = new SpreadsheetDetails(sheetName: "Sheet1", headerRow: 1, dataStartRow: 2, startColumn: 1);

var transformationDetails = new TransformationDetails(
    baseUri: new Uri("http://example.com/"),
    predicateBaseUri: new Uri("http://example.com/predicates/"),
    IdentifierSegment: new TargetPathSegment("id", "identifier"),
    targetPathSegments: new List<TargetPathSegment> { new TargetPathSegment("name", "name") },
    outputFormat: RdfFormat.Turtle
);
```

Finally, call the TransformSpreadsheet method to perform the transformation:

```csharp
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(spreadsheetContent));
string rdfOutput = transformerService.TransformSpreadsheet(spreadsheetDetails, transformationDetails, stream);
```

## API Reference
### ITransformerService
`TransformSpreadsheet(SpreadsheetDetails spreadsheetDetails, TransformationDetails transformationDetails, Stream content)`

Transforms the input spreadsheet into RDF format based on the provided transformation details.

### SpreadsheetDetails
`SpreadsheetDetails(string sheetName, int headerRow, int dataStartRow, int startColumn)`
Constructor to create a new SpreadsheetDetails object.

* `sheetName`: The name of the sheet containing the data in the input spreadsheet.
* `headerRow`: The row number containing the column headers in the input spreadsheet (1-based index).
* `dataStartRow`: The row number where the data starts in the input spreadsheet (1-based index).
* `startColumn`: The column number where the data starts in the input spreadsheet (1-based index).

### TransformationDetails
`TransformationDetails(Uri baseUri, Uri predicateBaseUri, TargetPathSegment? IdentifierSegment, List<TargetPathSegment> targetPathSegments, RdfFormat outputFormat)`

Constructor to create a new TransformationDetails object.

 * `baseUri`: The base URI for creating RDF individuals.
 * `predicateBaseUri`: The base URI for creating RDF predicates.
 * `IdentifierSegment`: An optional TargetPathSegment for the identifier of RDF individuals. If specified, the subjects for the resulting RDF triples will be created using baseUri + segment + value in the cell gotten from the column matching target. If not specified, the subjects will be created using baseUri + a GUID.
 * `targetPathSegments`: A list of TargetPathSegment objects to define the RDF individuals' properties.
 * `outputFormat`: The desired RDF format for the output (Turtle, Trig, or JSON-LD).

### TargetPathSegment
`TargetPathSegment(string target, string segment)`

Constructor to create a new TargetPathSegment object.

 * `target`: The property name of the literal to turn into an RDF individual. When transforming a spreadsheet, this will refer to a column name.
 * `segment`: The URI segment to append to the base URI, enabling the creation of specific namespaces for different individuals.

### RdfFormat
Enum with the following values: Turtle, Trig, Jsonld

## Support
For questions, issues, or feature requests, please create an issue on the GitHub repository or submit a pull request if you have any improvements to contribute.

## License
IntoRdf is released under the MIT License. See the LICENSE file for more information.

## Contributing
Contributions are welcome! Please follow these steps to contribute:

Fork the repository on GitHub.
Clone your fork and create a new branch for your feature or bugfix.
Commit your changes to the new branch.
Push your changes to your fork.
Open a pull request from your fork to the original repository.
Please ensure that your code follows the existing style and structure, and add tests where necessary.

## Changelog
For a detailed list of changes in each version, please refer to the CHANGELOG file.
