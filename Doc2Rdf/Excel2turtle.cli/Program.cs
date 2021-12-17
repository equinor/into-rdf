using Excel2Turtle.Mel;
using Excel2Turtle.Interfaces;
using Excel2Turtle.Capacities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Excel2Turtle.Infrastructure.Repository;
using Excel2Turtle.Infrastructure;
using Excel2Turtle.Core.Entities;
using Excel2Turtle.Lib;

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

            var content = Excel2TtlMapper.Initialize(fileName);
            var ttl = Excel2TtlMapper.Transform(content);

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
