using Api;
using Api.Utils.Cors;
using Api.Utils.Mvc;
using Api.Utils.Setups;
using Api.Utils.Swagger;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logConfiguration) =>
{
    logConfiguration.ConfigureBaseLogging(context);
    logConfiguration.AddApplicationInsightsLogging(services);
});

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddFusekiApis(builder.Configuration)
    .AddDownstreamWebApi(ApiKeys.CommonLib, builder.Configuration.GetSection(ApiKeys.CommonLib))
    .AddInMemoryTokenCaches();

builder.Services.AddApplicationInsightsTelemetry();

builder.SetupCustomSwagger();

builder.Services.AddControllers();
builder.Services.AddMvc(options => options.Filters.Add<SplinterExceptionActionFilter>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddServices();
builder.Services.SetupAuthorization();
builder.Services.AddApplicationInsightsTelemetry();
builder.SetupKeyVault();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.SetupCustomSwaggerUi(builder.Configuration);

app.SetupCors(builder);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.AddEndpoints();
app.MapControllers();

app.Run();