using System;
using System.IO;
using Doc2Rdf.Library.Extensions.DependencyInjection;
using Doc2Rdf.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Excel2Rdf.Cli;

class Program
{
    static int Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
                {
                    services.AddDoc2RdfLibraryServices();
                }
            ).Build();

        try
        {
            string outputDir = "output";
            string fileOrDir = args[0];
            if (args.Length != 1)
            {
                Console.WriteLine("Wrong number of input args. Please enter File or Directory");
                return 0;
            }

            CreateOutputDirectory(outputDir);

            IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var melService = provider.GetRequiredService<IMelTransformer>();

            if (Path.HasExtension(fileOrDir))
            {
                TransformFile(melService, fileOrDir);
            }
            else
            {
                foreach (var fileName in Directory.EnumerateFiles(fileOrDir))
                {
                    TransformFile(melService, fileName);
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Something went south! {ex.Message}");
        }

        return 0;
    }

    private static void TransformFile(IMelTransformer melTransformer, string fileName)
    {
        Console.WriteLine($"Transforming: {fileName}");
        var ttl = string.Empty;

        using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
        {
            ttl = melTransformer.Transform(stream, fileName);
        }

        var outputFile = $"output/{Path.GetFileNameWithoutExtension(fileName)}.ttl";
        File.WriteAllText(outputFile, ttl);
    }

    private static void CreateOutputDirectory(string outputDir)
    {
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }
}
