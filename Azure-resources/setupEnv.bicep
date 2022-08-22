param resourcePrefix string
param sku string
param location string = resourceGroup().location
param env string

var dotnetVersion = 'v6.0'
var linuxFxVersion = 'DOTNETCORE|6.0'
var dugtrioGroupId = '5cb080af-069d-47db-8675-67efa584f59c'
var loudredGroupId = 'bdf2d33e-44a0-4774-9a11-204301b8e502'

var resourceTags = {
  Product: 'Spine Splinter'
  Team: 'Dugtrio'
  Env: env
}

resource StorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${env}spinesplinterstorage'
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
  name: '${resourcePrefix}-insights'
  location: location
  tags: resourceTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource AppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${resourcePrefix}-plan'
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

var melFusekiSetting = {
  name: 'Servers__Dugtrio__BaseUrl'
  value: 'https://${env}-mel-fuseki.azurewebsites.net'
}

var tieMelAdadpterConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${TieMelAdapterStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(TieMelAdapterStorageAccount.id, TieMelAdapterStorageAccount.apiVersion).keys[0].value}'

resource Api 'Microsoft.Web/sites@2021-03-01' = {
  name: '${resourcePrefix}-api'
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
      linuxFxVersion: linuxFxVersion
      http20Enabled: true
      appSettings: [
        {
          name: 'ApplicationInsights__ConnectionString'
          value: ApplicationInsights.properties.ConnectionString
        }
        melFusekiSetting
      ]
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
  name: '${resourcePrefix}-func-plan'
  location: location
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1'
  }
}

var serviceBusEndpoint = '${ServiceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'

resource AzFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: '${resourcePrefix}-func'
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
        melFusekiSetting
      ]
    }
  }
}

resource KeyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: '${resourcePrefix}-vault'
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
