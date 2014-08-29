using Booster.Helpers;
using System;
using System.IO;
using System.Reflection;

namespace Booster
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = CommandLineParser.Parse(args);
            if (config.ContainsKey("pack"))
            {
                if (!config.ContainsKey("img"))
                {
                    Console.WriteLine("Not found parameter 'img' with path to images folder");
                }
                if (!config.ContainsKey("imgout"))
                {
                    Console.WriteLine("Not found parameter 'imgout' with path to output image folder");
                }
                if (!config.ContainsKey("asm"))
                {
                    Console.WriteLine("Not found parameter 'asm' with path to dll's folder");
                }
                AssemblyProvider.PackAssemblies(config["img"], config["imgout"], config["asm"]);
            }
            else
            {
                string imagesFolder;
                if (config.ContainsKey("img"))
                {
                    imagesFolder = config["img"];
                }
                else
                {
                    imagesFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                var loadedAssemblies = AssemblyProvider.LoadAssemblies(imagesFolder);
                loadedAssemblies.CallMain();
            }
            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
        }
    }
}
