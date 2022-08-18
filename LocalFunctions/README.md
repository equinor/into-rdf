# RUNNING SPLINTER FUNCTIONS LOCALLY

## Prerequisites

### Azurite 
Install the Azurite extension. It's an emulator to run local storages.

### Azure storage 
Install the Azure storage extension to access the emulated storages

Under the Azure tab to the left find **Workspace**. **Local Emulator** should appear underneath **Attached Storage Accounts**
Start both the Blob and Queue emulators. 

### Func cli 
Install [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash) (> v.4). The Azure Function cli is used below to start the local functions.

### Data
The function requires the data from *dugtrio-experimental/ontologies/data/facilityIdentifiers.ttl*. If these are not present the function will fail to retrieve the necessary Facility Id.

## Running Local functions
Functions in the LocalFunctions project are the previous clis, rewritten as function to enable querying Fuseki to enable the revision chain.

Drop the file you want to transform in [spreadsheet-data](https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd2daf888-a9d5-486b-86d7-ce46f07d9de9%2FresourceGroups%2FDugtrio-Experimental%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fdugtrioexperimental/path/spreadsheet-data/etag/%220x8DA6FA1C2E8640B%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride~/false/defaultId//publicAccessVal/None)

Start the function
```
func start --verbose
```
After the function has started a request containing the minimum required 

```
 curl --request POST http://localhost:7071/api/Excel2Rdf --data '{"FileName": "C232-AI-R-LA-00001_01_R_20210208.xlsx", "DocumentProjectId":"C232", "DataSource": "mel", "SheetName": "mel", "RevisionName": "01", "RevisionDate": "2021-02-08"}'
```

View the result in [transformed-data](https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd2daf888-a9d5-486b-86d7-ce46f07d9de9%2FresourceGroups%2FDugtrio-Experimental%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fdugtrioexperimental/path/transformed-data/etag/%220x8DA6FD40400D742%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride~/false/defaultId//publicAccessVal/None)

## Running TieMelConsumer function locally

### Prerequisites 
To run the TieMelConsumer function create a blob container named **prodmeladapterfiles**

### Start and run
Start the functions using the func cli as above.

Drop an Excel file, together with its associated TieMessage xml file, into the **prodmeladapterfiles** container. The function execution will start when the container contains exactly two files.