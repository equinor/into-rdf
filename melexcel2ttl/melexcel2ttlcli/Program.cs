using Melexcel2ttl;
using System;
using System.IO;
using System.Text;

namespace melexcel2ttlcli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            string fileOrDir = args[0];

            if (Path.HasExtension(fileOrDir))
            {
                var fileName = fileOrDir;
                TransformFile(fileName);
            }
            else
            {
                foreach (var fileName in Directory.EnumerateFiles(fileOrDir))
                {
                    TransformFile(fileName);
                }
            }
        }

        private static void TransformFile(string fileName)
        {
            if (!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }
            var outputFile = $"output/{Path.GetFileNameWithoutExtension(fileName)}.ttl";
            var ttl = TransformXlsx2Ttl(fileName);
            File.WriteAllText(outputFile, ttl);
        }

        private static string TransformXlsx2Ttl(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open))
            {
                var mapper = new Xslx2TtlMapper();
                return mapper.Map(fileName, fileStream);
            }
        }
    }
}
