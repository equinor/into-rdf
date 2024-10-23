# Current State
This project is not actively developed.
However, package upgrades and security issues are taken care of as they appear.

# into-rdf
This repository contains code for IntoRdf nuget package. It is the code itself together with a cli and a test project, both is intended to ease development and only the nuget pacakge itself is published.

IntoRdf is a NuGet package for transforming Excel (XLSX) files into RDF (Resource Description Framework) format. This package helps developers easily convert spreadsheet data into RDF triples using a simple and convenient API. To read more see [nuget readme](IntoRdf/docs/README.md)

## Release
To deploy a new release to nuget.org, create a new release on the main branch with the appropriate semver as the name, i.e. 1.0.0, **NOT** v1.0.0 . The name will be used as the nuget package version number, rather than the version in the .csproj-file.

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
