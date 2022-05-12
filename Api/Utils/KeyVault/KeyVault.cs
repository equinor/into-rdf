using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Api.Utils.KeyVault;

public static class KeyVault
{
    public static WebApplicationBuilder? SetupKeyVault(this WebApplicationBuilder? builder)
    {
        if (builder is null) return builder;
        var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");
        var credentials = new DefaultAzureCredential();
        var secretClient = new SecretClient(keyVaultUri, credentials);
        builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        return builder;
    }
}
