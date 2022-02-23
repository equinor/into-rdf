using System;
using System.IO;
using Doc2Rdf.Library;

namespace Excel2Turtle.Cli
{
    class Program
    {
        static int Main(string[] args)
        {

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
                
                if (Path.HasExtension(fileOrDir))
                {
                    TransformFile(fileOrDir);
                }
                else
                {
                    foreach (var fileName in Directory.EnumerateFiles(fileOrDir))
                    {
                        TransformFile(fileName);
                    }
                }                
           }

          catch (Exception ex)
            {
                Console.WriteLine($"Something went south! {ex.Message}");
            }

            return 0;
        }

        private static void TransformFile(string fileName)
        {
            Console.WriteLine($"Transforming: {fileName}");
            var ttl = string.Empty;

            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                ttl = MelTransformer.Transform(stream);
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
}
