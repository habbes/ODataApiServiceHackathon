using Hackathon2020.Poc01.Models;
using Microsoft.EntityFrameworkCore;

namespace Hackathon2020.Poc01.Data
{
    public class InMemoryDbContext : DbContext
    {
        public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}
