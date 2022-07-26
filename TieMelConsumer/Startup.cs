using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using TieMelConsumer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Azure;

[assembly: FunctionsStartup(typeof(Startup))]

namespace TieMelConsumer;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
            .SetBasePath(builder.GetContext().ApplicationRootPath)
            .AddJsonFile("settings.json")
            // .AddCommonSettings()
            .AddEnvironmentVariables()
            .AddJsonFile("local.settings.json", optional: true);
        AddKeyVault(builder.ConfigurationBuilder);
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        var context = builder.GetContext();
        var services = builder.Services;
        var configuration = context.Configuration;
        
        // avoid calling builder.Services.AddAuthentication() since it overrides internal Azure Function authentication
        // https://github.com/AzureAD/microsoft-identity-web/issues/1548
            services.AddMicrosoftIdentityWebApiAuthentication(configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddFusekiApis(configuration)
            .AddInMemoryTokenCaches();

        services.AddServices();
        services.AddServiceBusClient(configuration);

        services.AddAzureClients(opt =>
        {
            opt.AddBlobServiceClient(configuration["AzureWebJobsStorage"]);
        });
    }

    private static void AddKeyVault(IConfigurationBuilder config)
    {
        var builtConfig = config.Build();
        var keyVaultUri = new Uri($"https://{builtConfig["KeyVaultName"]}.vault.azure.net/");
        var credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            { ExcludeSharedTokenCacheCredential = true });
        var secretClient = new SecretClient(keyVaultUri, credentials);
        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    }
}