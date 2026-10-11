using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Models;

namespace PosSystem.Api.Data
{
    public class PosDbContext(DbContextOptions<PosDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<StockMovement> StockMovements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(10, 2);
                entity.Property(p => p.Sku).HasMaxLength(50);
                entity.Property(p => p.Name).HasMaxLength(200);
                entity.HasIndex(p => p.Sku).IsUnique();
            });
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.Property(sm => sm.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(sm => sm.Reason).HasMaxLength(500);
                entity.HasOne(sm => sm.Product)
                      .WithMany()
                      .HasForeignKey(sm => sm.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
