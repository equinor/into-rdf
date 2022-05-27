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
    string tableName = string.Empty;
    if (args.Length == 1)
    {
        tableName = "Item";
    }
    else if (args.Length == 2)
    {
        tableName = args[1];
    }
    else 
    {
        Console.WriteLine("Wrong number of input args. Please enter Platform identifier, i.e. Grane, Gudrun, Gina Krog, Aasta Hanstein, Valemon");
        return 0;
    }

    CreateOutputDirectory(outputDir);

    IServiceScope serviceScope = host.Services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var shipWeightTransformer = provider.GetRequiredService<IShipWeightTransformer>();

    var plantId = ShipWeightDBReader.GetPlantId(facilityName);

    DataTable inputData = args[1].ToLower() == "as-built" ?
                 ShipWeightDBReader.GetAsBuiltData(facilityName) :
                 ShipWeightDBReader.GetData(facilityName, tableName);

    TransformData(shipWeightTransformer, inputData, facilityName, plantId, tableName);
}

catch (Exception ex)
{
    Console.WriteLine($"Something went south! {ex.Message}");
}

return 0;

static void TransformData(IShipWeightTransformer shipWeightTransformer, DataTable inputData, string facilityName, string plantId, string tableName)
{
    var ttl = shipWeightTransformer.Transform(facilityName, plantId, inputData);

    var outputFile = $"output/shipweight-{facilityName}-{tableName}-{DateTime.Now.ToString("yy-MM-dd")}.ttl";
    File.WriteAllText(outputFile, ttl);
}

static void CreateOutputDirectory(string outputDir)
{
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }
}