using EdmObjectsGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Please specify CSDL file path");
                Environment.Exit(1);
            }

            var csdlFileName = args[0];
            string dbContextName = csdlFileName.Split('\\').Last().Replace(".xml", "");
            var generator = new DbContextGenerator();
            generator.GenerateContextAssembly(csdlFileName, dbContextName);
        }
    }
}
