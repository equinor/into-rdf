using Api;

var builder = WebApplication.CreateBuilder(args);

// Serilog and ApplicationInsights
builder.Host.AddLogging();

// Add JWT auth and downstreamAPI token propagation
builder.Services.AddMSFTAuthentication(builder.Configuration);

// Regular aspnetcore authorization w/ additional fallback to deny access to
// routes that do not have explicit authorization requirements
builder.Services.AddFallbackAuthorization();

// Add swagger API-doc autogen service w/ oauth2 auth flow configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwagger(builder.Configuration);

// Add domain services for API logic
builder.Services.AddAPIServices();

// Add MVC Controllers & Minimal API endpoints
builder.Services.AddAPIEndpoints();


var app = builder.Build();

// Configure Swagger endpoints and auth
app.UseSwaggerWithUI(app.Configuration);
// Configure CORS from appsettings
app.UseCorsFromAppsettings(app.Configuration, app.Environment);

// Make extra sure everything is on the up'n'up
app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.UseExceptionHandling();

app.MapAPIEndpoints();

app.Run();

