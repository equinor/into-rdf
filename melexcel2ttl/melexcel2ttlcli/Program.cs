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
            string path = args[0];
            string ttl = null;
            int index = 0;
            foreach (var fileName in Directory.EnumerateFiles(path))
            {
                index += 1;
                var outputFile = $"output_{index}.ttl";
                ttl = Transformation(fileName);
                File.WriteAllText(outputFile, ttl);
            }
        }

        private static string Transformation(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open))
            {
                var mapper = new Xslx2TtlMapper();
                return mapper.Map(fileName, fileStream);
            }
        }
    }
}
