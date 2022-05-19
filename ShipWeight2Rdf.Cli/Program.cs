using Doc2Rdf.Library.Extensions.DependencyInjection;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShipWeight.Database;
using System.Data;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
        {
            services.AddDoc2RdfLibraryServices();
        }
    ).Build();

try
{
    string outputDir = "output";
    string facilityName = args[0].Contains("Drift") ? args[0] : $"{args[0]}_Drift";
    if (args.Length != 1)
    {
        Console.WriteLine("Wrong number of input args. Please enter Platform identifier, i.e. Grane, Gudrun, Gina Krog, Aasta Hanstein, Valemon");
        return 0;
    }

    CreateOutputDirectory(outputDir);

    IServiceScope serviceScope = host.Services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var shipWeightTransformer = provider.GetRequiredService<IShipWeightTransformer>();

    var inputData = ShipWeightDBReader.GetData(facilityName);

    TransformData(shipWeightTransformer, inputData, facilityName);
}

catch (Exception ex)
{
    Console.WriteLine($"Something went south! {ex.Message}");
}

return 0;

static void TransformData(IShipWeightTransformer shipWeightTransformer, DataSet inputData, string facilityName)
{
    var ttl = shipWeightTransformer.Transform(facilityName, inputData);

    var outputFile = $"output/shipweight-{facilityName}-{DateTime.Now.ToString("yy-MM-dd")}.ttl";
    File.WriteAllText(outputFile, ttl);
}

static void CreateOutputDirectory(string outputDir)
{
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }
}