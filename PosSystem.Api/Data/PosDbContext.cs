using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Models;

namespace PosSystem.Api.Data
{
    public class PosDbContext(DbContextOptions<PosDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(10, 2);
                entity.Property(p => p.Sku).HasMaxLength(50);
                entity.Property(p => p.Name).HasMaxLength(200);
                entity.HasIndex(p => p.Sku).IsUnique();
            });
        }
    }
}
