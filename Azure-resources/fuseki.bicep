param name string
param env string
param clientId string
param environmentTag string
param fusekiConfig string
param buildId string
param sku string = 'B1'
param tenantId string = '3aa4a235-b6e2-48d5-9195-7fcf05b459b0'
param location string = resourceGroup().location

var resourceTags = {
  Product: 'Spine ${name} fuseki'
  Team: 'Dugtrio'
  Env: env
  BuildId: buildId
}

var shortResourcePrefix = '${environmentTag}${name}fuseki'
var longResourcePrefix = '${environmentTag}-${name}-fuseki'

var fileShareName = 'fusekifileshare'

resource AcrPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: 'acr-pull-identity'
  scope: resourceGroup('spine-acr')
}

resource FusekiStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${shortResourcePrefix}store'
  location: location
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
  }
}

resource FusekiFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2021-08-01' = {
  name: '${FusekiStorageAccount.name}/default/${fileShareName}'
}

resource FusekiPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
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

resource Fuseki 'Microsoft.Web/sites@2021-03-01' = {
  name: longResourcePrefix
  kind: 'app'
  tags: resourceTags
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${AcrPullIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: FusekiPlan.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|spineacr.azurecr.io/spinefuseki:latest'
      http20Enabled: true
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: AcrPullIdentity.properties.clientId
      appCommandLine: '--conf /fuseki/config/${fusekiConfig}'
      azureStorageAccounts: {
        '${fileShareName}': {
          type: 'AzureFiles'
          accountName: FusekiStorageAccount.name
          shareName: fileShareName
          mountPath: '/meltdb2'
          accessKey: listKeys(FusekiStorageAccount.id, FusekiStorageAccount.apiVersion).keys[0].value
        }
      }
    }
  }
  dependsOn: [
    FusekiFileShare
  ]
}

resource FusekiAuthSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'authsettingsV2'
  kind: 'string'
  parent: Fuseki
  properties: {
    globalValidation: {
      unauthenticatedClientAction: 'Return401'
      requireAuthentication: true
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          clientId: clientId
          openIdIssuer: 'https://sts.windows.net/${tenantId}/v2.0'
        }
      }
    }
  }
}
