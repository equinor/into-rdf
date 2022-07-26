using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using Services.DependencyInjection;
using Services.TransformationServices.SpreadsheetTransformationServices;

namespace Excel2Rdf.Cli;

class Program
{
    static int Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
                {
                    services.AddLogging(options => options.AddConfiguration(context.Configuration));
                    services.AddSplinterServices();
                })
            .Build();

        try
        {
            //TODO - Add input arguments, DocumentProject, RevisionName, RevisionDate
            if (args.Length != 2)
            {
                Console.WriteLine("Wrong number of input args. Please enter datasource (mel, linelist, stream) and File or Directory");
                return 0;
            }

            string outputDir = "output";
            string dataSource = args[0];
            string fileOrDir = args[1];

            CreateOutputDirectory(outputDir);

            IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var services = provider.GetServices<ISpreadsheetTransformationService>();
            var service = services.FirstOrDefault(x => x.GetDataSource() == dataSource) ?? 
                                        throw new ArgumentException($"Transformer of type {dataSource} not available");

            if (Path.HasExtension(fileOrDir))
            {
                
                TransformFile(service, fileOrDir);
            }
            else
            {
                foreach (var fileName in Directory.EnumerateFiles(fileOrDir))
                {
                    TransformFile(service, fileName);
                }
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Something went south! {ex.Message}");
        }

        return 0;
    }

    private static void TransformFile(ISpreadsheetTransformationService transformer, string fileName)
    {
        Console.WriteLine($"Transforming: {fileName}");
        var ttl = string.Empty;

        using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
        {
            ttl = transformer.Transform(stream, fileName);
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
