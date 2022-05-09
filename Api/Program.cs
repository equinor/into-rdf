using Api;
using Api.Utils.Cors;
using Api.Utils.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddApplicationInsightsTelemetry();

builder.SetupCustomSwagger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSplinterServices();
builder.Services.AddAzureClients(azureBuilder =>
{
    azureBuilder.AddSecretClient(new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.SetupCustomSwaggerUi(builder.Configuration);

app.SetupCors(builder);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
