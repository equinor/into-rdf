using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
namespace Api;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddLogging(this IHostBuilder builder)
    {
        builder.ConfigureServices(s => s.AddApplicationInsightsTelemetry());
        builder.UseSerilog((context, services, logConfiguration) =>
        {
            {
                var logLevel = context.HostingEnvironment.IsProduction()
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;

                var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
                var asmTypeName = Assembly.GetEntryAssembly()?.GetName().Name;

                if (entryAssemblyName == null || asmTypeName == null)
                {
                    throw new Exception("Fatal: Unable to get assembly info when configuring logging");
                }

                logConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", logLevel)
                    .MinimumLevel.Override("System", logLevel)
                    .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("AssemblyVersion", entryAssemblyName)
                    .Enrich.WithProperty("Application", asmTypeName)
                    .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName);
            }
            {
                var telemetryConfiguration = services.GetRequiredService<TelemetryConfiguration>();
                if (!string.IsNullOrWhiteSpace(telemetryConfiguration.ConnectionString))
                    logConfiguration.WriteTo.ApplicationInsights(
                        telemetryConfiguration,
                        TelemetryConverter.Traces);
                else
                    Console.WriteLine(
                        "Application Insights ConnectionString is missing, not sending logs to Application Insights");
            }
        });
        return builder;
    }

}
