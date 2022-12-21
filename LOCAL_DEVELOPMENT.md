# SpineSplinter
Orchestrator of data from Fuseki stores

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
