﻿using Api.Authorization.Handlers.Fallback;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;

namespace Api.Utils.Setups;

public static class Setups
{
    public static void SetupKeyVault(this WebApplicationBuilder builder)
    {
        var keyVaultName = builder.Configuration["KeyVaultName"];
        if (string.IsNullOrEmpty(keyVaultName))
        {
            throw new Exception("keyVaultName not set in configuration, see readme");
        }
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var credentials = new DefaultAzureCredential();
        var secretClient = new SecretClient(keyVaultUri, credentials);
        builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    }
    
    public static void SetupAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            //Setup safeguard if not Authorize or AllowAnonymous is present on endpoint
            var fallbackPolicyBuilder = new AuthorizationPolicyBuilder();
            fallbackPolicyBuilder.Requirements.Add(new FallbackSafeguardRequirement());
            var fallbackPolicy = fallbackPolicyBuilder.Build();
            options.FallbackPolicy = fallbackPolicy;

        });
            
        services.AddSingleton<IAuthorizationHandler, FallbackSafeguardHandler>();
    }
}
