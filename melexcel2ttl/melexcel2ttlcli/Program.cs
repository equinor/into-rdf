using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using melexcel2ttl;


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
                using (var memoryStream = new MemoryStream())
                {
                    var mapper = new Xslx2TttlMapper();
                    mapper.Map(fileName, fileStream, memoryStream);
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }
    }
}
