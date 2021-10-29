using Excel2ttl.Mel;
using Excel2ttl.Interfaces;
using Excel2ttl.Capacities;
using System;
using System.IO;
using System.Text;

namespace melexcel2ttlcli
{
    class Program
    {
        static int Main(string[] args)
        {
            string fileOrDir = args[0];
            string dataType = args[1];

            if (args.Length != 2)
            {
                Console.WriteLine("Wrong number of input args. Please enter File or Directory and Type of data (mel or capacities)");
                return 0;
            }

            if (!args[1].Equals("mel") && !args[1].Equals("capacities"))
            {
                Console.WriteLine("Wrong kind of data. Valid values: mel or capacities");
                return 0;
            }

            if (Path.HasExtension(fileOrDir))
            {
                var fileName = fileOrDir;
                TransformFile(fileName, dataType);
            }
            else
            {
                foreach (var fileName in Directory.EnumerateFiles(fileOrDir))
                {
                    Console.WriteLine($"Transforming: {fileName}");
                    TransformFile(fileName, dataType);
                }
            }
            return 0;
        }

        private static void TransformFile(string fileName, string dataType)
        {
            if (!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }
            var outputFile = $"output/{Path.GetFileNameWithoutExtension(fileName)}.ttl";
            var ttl = TransformXlsx2Ttl(fileName, dataType);
            File.WriteAllText(outputFile, ttl);
        }

        private static string TransformXlsx2Ttl(string fileName, string dataType)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open))
            {
                ITtlMapper mapper = new Mel2TtlMapper();
                switch (dataType)
                {
                    case "mel":
                        mapper = new Mel2TtlMapper();
                        break;
                    case "capacities":
                        mapper = new CapacitiesTtlMapper();
                        break;
                    default:
                        break;
                } 

                return mapper.Map(fileName, fileStream);
            }
        }
    }
}
