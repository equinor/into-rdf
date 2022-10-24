param environmentTag string
param sku string
param env string
param buildId string
param location string = resourceGroup().location

param fusekiParameters array = [
  {
    name: 'oldugtrio'
    clientId: env == 'prod' ? '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' : '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
  }
  {
    name: 'review'
    clientId: env == 'prod' ? '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' : '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
  }
  {
    name: 'krafla'
    clientId: env == 'prod' ? '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' : '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
  }
  {
    name: 'wisting'
    clientId: env == 'prod' ? '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' : '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
  }
  {
    name: 'main'
    clientId: env == 'prod' ? '6dbf2494-f87f-4d25-a9ee-891d262ece45' : '6dbf2494-f87f-4d25-a9ee-891d262ece45' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
  }
]

var dotnetVersion = 'v6.0'
var dugtrioGroupId = '5cb080af-069d-47db-8675-67efa584f59c'
var loudredGroupId = 'bdf2d33e-44a0-4774-9a11-204301b8e502'

var resourceTags = {
  Product: 'Spine Splinter'
  Team: 'Dugtrio'
  Env: env
  BuildId: buildId
}

var shortResourcePrefix = '${environmentTag}splinter'
var longResourcePrefix = '${environmentTag}-splinter'

var vaultName = '${longResourcePrefix}-vault'

resource StorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${shortResourcePrefix}storage'
  location: location
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    accessTier: 'Hot'
  }
}

resource ServiceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: 'spine-sbus'
  scope: resourceGroup('spine-servicebus')
}

resource TieMelAdapterStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: 'prodmeladapterstorageacc'
  scope: resourceGroup('prod-tie-mel-adapter')
}

resource ApplicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${longResourcePrefix}-insights'
  location: location
  tags: resourceTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource AppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${longResourcePrefix}-plan'
  location: location
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: sku
  }
}

var tieMelAdadpterConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${TieMelAdapterStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(TieMelAdapterStorageAccount.id, TieMelAdapterStorageAccount.apiVersion).keys[0].value}'

/*
  Example appsetting

  "Servers": [
    {
      "Name": "Dugtrio",
      "BaseUrl": "https://dev-dugtrio-fuseki.azurewebsites.net",
      "Scopes": "2ff9de24-0dba-46e0-9dc1-096cc69ef0c6/.default"
    },
    ...
  ]
*/

var fusekiSettings = [for i in range(0, length(fusekiParameters)): [
  {
    name: 'Servers__${i}__BaseUrl'
    value: 'https://${environmentTag}-${fusekiParameters[i].name}-fuseki.azurewebsites.net'
  }, {
    name: 'Servers__${i}__Scopes'
    value: '${fusekiParameters[i].clientId}/.default'
  }, {
    name: 'Servers__${i}__Name'
    value: '${fusekiParameters[i].name}'
  }
]]

resource Api 'Microsoft.Web/sites@2021-03-01' = {
  name: '${longResourcePrefix}-api'
  kind: 'app'
  tags: resourceTags
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: AppServicePlan.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      alwaysOn: false
      netFrameworkVersion: dotnetVersion
      linuxFxVersion: 'DOTNETCORE|6.0'
      http20Enabled: true
      appSettings: union([
        {
          name: 'ApplicationInsights__ConnectionString'
          value: ApplicationInsights.properties.ConnectionString
        }
        { name: 'KeyVaultName'
          value: vaultName
        }
      ], flatten(fusekiSettings))
      connectionStrings: [
        {
          name: 'SpineReviewStorage'
          type: 'Custom'
          connectionString: tieMelAdadpterConnectionString
        }
      ]
    }
  }
  dependsOn: []
}

resource FuncServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${longResourcePrefix}-func-plan'
  location: location
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

var serviceBusEndpoint = '${ServiceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'

resource AzFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: '${longResourcePrefix}-func'
  kind: 'functionapp,linux'
  tags: resourceTags
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: FuncServicePlan.id
    httpsOnly: true
    reserved: true
    dailyMemoryTimeQuota: 1000
    siteConfig: {
      netFrameworkVersion: dotnetVersion
      linuxFxVersion: 'dotnet|6'
      http20Enabled: true
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${StorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(StorageAccount.id, StorageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'TieMelAdapterStorage'
          value: tieMelAdadpterConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: ApplicationInsights.properties.InstrumentationKey
        }
        {
          name: 'ConnectionStrings__ServiceBus'
          value: listKeys(serviceBusEndpoint, ServiceBusNamespace.apiVersion).primaryConnectionString
        }
      ]
    }
  }
}

resource KeyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: vaultName
  location: location
  tags: resourceTags
  properties: {
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: Api.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: AzFunction.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: loudredGroupId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: dugtrioGroupId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
        }
      }
    ]
  }
}

module fuskis './fuseki.bicep' = [for parameter in fusekiParameters: {
  name: parameter.name
  params: {
    buildId: buildId
    env: env
    environmentTag: environmentTag
    clientId: parameter.clientId
    name: parameter.name
    fusekiConfig: parameter.fusekiConfig
    location: parameter.location
  }
}]
