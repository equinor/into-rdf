# Splinter

Splinter is a service for orchestrating RDF data. 

The solution consists of:
- an Api 
    - endpoints where RDF records can be managed
    - endpoints to make the RDF data available to end-user applications
- a routing layer, communicating with the triplestores used as storage. At the moment only Apache Jena Fuseki triplestores are used.

## Local development
Use appsettings.Development.json when running splinter locally (.gitignored).

### Configure downstream servers

If testing against
    * a fuseki running at https://localhost:3030
    * that should be accessable using name testfuseki
    * is protected with an app registration in azure with the following id: c0ffec0ffec0ffec0ffec0ffec0ffec0ffec0ffe use a configuration like this:

```
...
"Servers": [
    {
        "name": "testfuseki",
        "baseUrl": "https://localhost:3030",
        "scopes": "c0ffec0ffec0ffec0ffec0ffec0ffec0ffec0ffe/.default"
    }
]
...
```

### Configure keyvault

The development secret required to access downstream fusekis is stored in a keyvault called dev-splinter-vault. This should be configured like this:

```
...
"keyVaultName": "dev-splinter-vault"
...
```

All developers should have access to this keyvault.

### Update dependency scripts

Instead of going through all the dependabot PRs one-by-one the following script can be used to update all dependencies.
```
for csproj in `find . -iname \*.csproj -o -iname \*.fsproj`; do dotnet list $csproj package | awk '$1==">"{print $2}' | xargs -i dotnet add $csproj package {}; done
```
Dependabot will rebase all its PRs when the resulting PR is merged to main.

