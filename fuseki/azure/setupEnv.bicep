param env string
param clientId string = 'NOT_CONFIGURED'
param appSvcSku string = 'P1v2'
param appSvcSkuTier string = 'PremiumV2'
param tenantId string = '3aa4a235-b6e2-48d5-9195-7fcf05b459b0'
param location string = resourceGroup().location

var resourcePrefix = '${env}-dugtriofuseki'
var webAppName = resourcePrefix
var planName = '${resourcePrefix}-plan'
var clientSecretName = 'AAD_SECRET'
var resourceTags = {
  Product: 'Dugtrio Fuseki'
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
  }
  properties: {
    serverFarmId: planName
    httpsOnly: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|spineacr.azurecr.io/fuseki:${env}'
      http20Enabled: true
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: 'GUID'
      appSettings: []
    }
  }
  dependsOn: [
    FusekiPlan
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
