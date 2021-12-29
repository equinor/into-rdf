﻿using System;
using System.IO;
using Doc2Rdf.Library;
using Excel2Turtle.Core.Entities;


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

            var content = new SpreadsheetContent();
            content.Workbook = fileName;

            var settingContent = File.ReadAllText("..\\Excel2Turtle.Infrastructure\\settings.json");
            content.RdfSettings = Doc2RdfTransformer.GetRdfSettings(settingContent);

            content.SpreadsheetDetails = Doc2RdfTransformer.GetSpreadsheetDetails(fileName, content.RdfSettings);

            content.DataTable = Doc2RdfTransformer.GetSpreadsheetData(fileName, content.SpreadsheetDetails);

            var ttl = Doc2RdfTransformer.Transform(content);

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
