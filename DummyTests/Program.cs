using System;
using EdmObjectsGenerator;

namespace DummyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");
            DbContextGenerator generator = new DbContextGenerator();
            var context = generator.GenerateDbContext(@"SampleModel.xml");
        }
    }
}
