using Hackathon2020.Poc01.Lib;
using System.ComponentModel.DataAnnotations;

namespace Hackathon2020.Poc01.Models
{
    [DynamicController(route: "Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
