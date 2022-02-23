using Doc2Rdf.Library;

try
{
    string outputDir = "output";
    string facilityName = args[0];
    if (args.Length != 1)
    {
        Console.WriteLine("Wrong number of input args. Please enter Platform identifier, i.e. Grane, Gudrun, Gina Krog, Aasta Hanstein, Valemon");
        return 0;
    }

    CreateOutputDirectory(outputDir);

    TransformData($"{facilityName}_Drift");
}

catch (Exception ex)
{
    Console.WriteLine($"Something went south! {ex.Message}");
}

return 0;

static void TransformData(string facilityName)
{
    var shipWeightTransformer = new ShipWeightTransformer();
    var ttl = shipWeightTransformer.Transform(facilityName);

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