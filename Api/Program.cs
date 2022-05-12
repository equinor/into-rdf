using Api;
using Api.Utils.Cors;
using Api.Utils.KeyVault;
using Api.Utils.Mvc;
using Api.Utils.Swagger;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddApplicationInsightsTelemetry();

builder.SetupCustomSwagger();

builder.Services.AddControllers();
builder.Services.AddMvc(options => options.Filters.Add<SplinterExceptionActionFilter>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSplinterServices(builder.Configuration);

builder.SetupKeyVault();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.SetupCustomSwaggerUi(builder.Configuration);

app.SetupCors(builder);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
