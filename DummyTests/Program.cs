using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using AutoBogus;
using EdmObjectsGenerator;

namespace DummyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("");
            //DbContextGenerator generator = new DbContextGenerator();
            //var context = generator.GenerateDbContext(@"SampleModel.xml");

            GenerateData();
        }

        static void GenerateData()
        {
            var argType = typeof(Action<>).MakeGenericType(typeof(IAutoGenerateConfigBuilder));
            var person = AutoFaker.Generate<Person>();
            var generatMethod = typeof(AutoFaker).GetMethod("Generate", new[] { argType });
            var generatePerson = generatMethod.MakeGenericMethod(typeof(Person));
            object p = generatePerson.Invoke(null, new object[] { null }) ;
        }

        
    }

    class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Person> Friends { get; set; }
        public List<string> Emails { get; set; }
        public Address Address { get; set; }
    }

    class Sport
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [ComplexType]
    class Address
    {
        public string City { get; set; }
        public string Country { get; set; }
    }
}
