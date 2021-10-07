# Upload ttl file to fuseki
az login
TOKEN=$(az account get-access-token --resource 2ff9de24-0dba-46e0-9dc1-096cc69ef0c6 | jq -r .accessToken)
curl -X POST --data-binary @/fuseki/melttl/mel.ttl https://dugtriofuseki.azurewebsites.net/ds/data -H "Authorization: Bearer $TOKEN"  -H "Content-type: text/turtle" -v

# Tailing fuseki log
az login
az webapp log tail --name dugtriofuseki --resource-group Dugtrio-Experimental

# Commands for pushing new docker image to azure
az login
Elevate azure rights using PIM
az acr login --name spineacr
./publish.sh
