param env string
param clientId string = 'NOT_CONFIGURED'
param appSvcSku string = 'P1v2'
param appSvcSkuTier string = 'PremiumV2'
param tenantId string = '3aa4a235-b6e2-48d5-9195-7fcf05b459b0'
param location string = resourceGroup().location

var resourcePrefix = '${env}-dugtriofuseki'
var webAppName = resourcePrefix
var planName = '${resourcePrefix}-plan'
var storageAccountName = '${env}dugtriofusekistorage'
var fileShareName = 'fusekifileshare'
var clientSecretName = 'AAD_SECRET'
var resourceTags = {
  Product: 'Dugtrio Fuseki'
}

resource AcrPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: 'acr-pull-identity'
  scope: resourceGroup('spine-acr')
}

resource FusekiStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
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
  name: planName
  location: location
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: appSvcSku
    tier: appSvcSkuTier
  }
}

resource Fuseki 'Microsoft.Web/sites@2021-03-01' = {
  name: webAppName
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
    serverFarmId: planName
    httpsOnly: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|spineacr.azurecr.io/fuseki:latest'
      // linuxFxVersion: 'DOCKER|spineacr.azurecr.io/fuseki:${env}'
      http20Enabled: true
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: AcrPullIdentity.properties.clientId
      appCommandLine: '--conf /fuseki/config/mel_persisted_reasoning_config.ttl'
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
    FusekiPlan
    FusekiFileShare
  ]
}

resource AuthSettings 'Microsoft.Web/sites/config@2021-03-01' = {
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
          clientSecretSettingName: clientSecretName
          openIdIssuer: 'https://sts.windows.net/${tenantId}/v2.0'
        }
      }
    }
  }
}
