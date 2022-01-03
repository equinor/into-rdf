# Loading MEL data to Fuseki
At the moment the MEL data are stored as turtle files. Here's how to upload them to the in-memory Fusekis

## Where are the files?
The turtle files are stored at the **dugrtrioexperimental** storage account. A list of the containers are given here: 
[dugtrioexperimental containers](https://portal.azure.com/#@StatoilSRM.onmicrosoft.com/resource/subscriptions/d2daf888-a9d5-486b-86d7-ce46f07d9de9/resourceGroups/Dugtrio-Experimental/providers/Microsoft.Storage/storageAccounts/dugtrioexperimental/containersList)

The **prod-mel-ttl** container contains the latest verified data set. A data set contains data from mel spreadsheets, capacity spreadsheet and the necessary vocabularies/ontologies. "Verified" means that Loudred has had a look at it.

The **dev-mel-ttl** container has the latest data. Only use when testing whether these data can replace the data in prod.

## How to upload?
Download the files in prod-mel-ttl. Open the Leonardo website linked to the Fuseki you're updating, i.e https://dugtrioleonardo.azurewebsites.net/ for ME Review in production or https://dugtrioleonardo-test.azurewebsites.net/ for ME Review in development.

Hit the Michelangelo turtle, select all the downloaded files and press "Upload files"

## Fancy a cli?
After downloading, the files in the directory can be curled as follows: 
````
az login

token=$(az account get-access-token --resource 2ff9de24-0dba-46e0-9dc1-096cc69ef0c6 | jq -r .accessToken)

ls | xargs -i curl -X POST --data-binary @{} https://dugtriofuseki.azurewebsites.net/ds/data -H "Authorization: Bearer $token" -H "Content-Type: text/turtle" -v
````

