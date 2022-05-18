using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Doc2Rdf.Library.Extensions.DependencyInjection;

namespace Doc2Rdf.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {  
        services.AddDoc2RdfLibraryServices();
    }
}