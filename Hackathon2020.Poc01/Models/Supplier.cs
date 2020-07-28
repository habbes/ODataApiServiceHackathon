using Hackathon2020.Poc01.Lib;
using System.ComponentModel.DataAnnotations;

namespace Hackathon2020.Poc01.Models
{
    [DynamicController(route: "Suppliers")]
    public class Supplier
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
    }
}
