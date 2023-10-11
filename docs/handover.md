# Handover of Into-RDF

Completion of all the items on the list below and reviewing of a pull request indicates a successful and complete handover.

- [x] Admin Role of Github Repo transferred to SSI (Spine Semantic Infrastucture)
  - [x] 1. SSI promoted to Admin
    - SSI team is promoted to Admin
  - [x] 2. Dugtrio demoted / removed from team access list
- [x] Ensure that the nuget publishing pipeline is owned by SSI
    Nuget is published using dotnet cli inside github action with a secret that we got from Markus
- [x] Ensure that snyk project is transferred to SSI
    - SSI has added IntoRdf to their organsization
    - Dugtrio has removed IntoRdf from their organization
    - Deleted dugtrios snyk token from github secret
    - Added SSIs newly generated snyk token to github secret
    - Verified that github action snyk scan still worked
- [x] Atleast one specific resource on the SSI team is onboarded to the XML Vs Excel Index problem.
      - Johannes, Henriette and Dag was onboarded to this problem 11. October 2023
- [x] Atleast one specific resource on the SSI team is onboarded to ° ² ³ Encoding problem.
      - Johannes, Henriette and Dag was onboarded to this problem 11. October 2023, see https://dev.azure.com/EquinorASA/Spine/_workitems/edit/118069
- [x] Dugtrio describes what they think is legacy code.
      - All AML related code
- [x] Dugtrio present their ideas for further work .
      - If datasheet / electtrical loadlist / other excel formats can go into this nuget package, we need some new structures in IntoRdf
- [x] Atleast one contribution to the repository has been made by a member of the SSI team.
  -  https://github.com/equinor/into-rdf/commit/41808d149e80e7c001829470b13328df34991c2f - Coauthored by Markus
- [x] At least one resource on the SSI team has released a new version of Into-RDF using ReleasePlease
  - Johannes, Henriette and Dag released version 0.3.6. There where some issues with releaseplease:
    - Automatic update of csproj did not work
    - We needed to included chores in releases to test this flow

Thanks to earlier work when the Nuget package was made this handover has had a head start and SSI is already up to speed on the inner workings of the functionality.
