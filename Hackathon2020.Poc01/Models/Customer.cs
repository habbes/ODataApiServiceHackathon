using System.ComponentModel.DataAnnotations;

namespace Hackathon2020.Poc01.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
