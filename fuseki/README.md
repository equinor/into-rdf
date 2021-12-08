# Fuseki setup
This repo / folder is ment to ease the work of publishing docker images based on remote fuseki docker files to Azure container registry.

## Build image
```
./build.sh
```

## Commands for pushing new docker image to azure
```
az login
# *Elevate azure rights using PIM*
az acr login --name spineacr
./publish.sh
```

## Configuring images
All parameters to the docker run command are forwarded to fuseki. This means that fuseki can be configured by passing parameters to docker run using these fuseki configuration options:

1. The command line, using --conf to provide a configuration file.
2. The command line, using arguments (e.g. --mem /ds or --tdb2 --loc DB2 /ds).

See https://jena.apache.org/documentation/fuseki2/fuseki-configuration.html

If the first one is chosen it must use one of the configuration files already copied to the server. See ./build.sh, or whatever script is runnign this at the time of reading, for name of config files. at Tue Dec 7 13:14:56 2021 this was `reasoning_config.ttl` and `no_reasoning_config.ttl` meaning these parameter could be given to docker and run locally like:

1a. `docker run --rm -p 8080:3030 fuseki --conf /fuseki/config/reasoning_config.ttl`
1b. `docker run --rm -p 8080:3030 -e JAVA_OPTIONS="-Xmx8g" fuseki --conf /fuseki/config/no_reasoning_config.ttl`

`-e JAVA_OPTIONS="-Xmx8g"` is for configuring more memory to jvm.

## Upload ttl file to fuseki
```
az login
TOKEN=$(az account get-access-token --resource 2ff9de24-0dba-46e0-9dc1-096cc69ef0c6 | jq -r .accessToken)
curl -X POST --data-binary @/fuseki/melttl/mel.ttl https://dugtriofuseki.azurewebsites.net/ds/data -H "Authorization: Bearer $TOKEN"  -H "Content-type: text/turtle" -v
```

## Tailing fuseki log
```
az login
az webapp log tail --name dugtriofuseki --resource-group Dugtrio-Experimental
```


