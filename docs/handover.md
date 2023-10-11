# Handover of Into-RDF

Completion of all the items on the list below and reviewing of a pull request indicates a successful and complete handover.

- [ ] Admin Role of Github Repo transferred to SSI (Spine Semantic Infrastucture)
  - [x] 1. SSI promoted to Admin
    - SSI team is promoted to Admin
  - [ ] 2. Dugtrio demoted / removed from team access list
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
- [ ] Atleast one specific resource on the SSI team is onboarded to ° ² ³ Encoding problem.
- [ ] Dugtrio describes what they think is legacy code.
- [ ] Dugtrio present their ideas for further work .
- [x] Atleast one contribution to the repository has been made by a member of the SSI team.
  -  https://github.com/equinor/into-rdf/commit/41808d149e80e7c001829470b13328df34991c2f - Coauthored by Markus
- [ ] At least one resource on the SSI team has released a new version of Into-RDF using ReleasePlease

Thanks to earlier work when the Nuget package was made this handover has had a head start and SSI is already up to speed on the inner workings of the functionality.