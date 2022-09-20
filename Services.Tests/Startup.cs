using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.DependencyInjection;

namespace Services.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddSplinterServices();
    }
}