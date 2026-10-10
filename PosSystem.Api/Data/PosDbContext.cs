using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Models;

namespace PosSystem.Api.Data
{
    public class PosDbContext(DbContextOptions<PosDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;
    }
}
