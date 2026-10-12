using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Models;

namespace PosSystem.Api.Data
{
    public class PosDbContext(DbContextOptions<PosDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<StockMovement> StockMovements { get; set; } = null!;
        public DbSet<Sale> Sales { get; set; } = null!;
        public DbSet<SaleLine> SaleLines { get; set; } = null!;

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
                entity.HasOne(m => m.Sale)
                      .WithMany()
                      .HasForeignKey(m => m.SaleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(s => s.Total).HasPrecision(10, 2);
                entity.Property(s => s.Change).HasPrecision(10, 2);
                entity.Property(s => s.VatAmount).HasPrecision(10, 2);
                entity.Property(s => s.AmountPaid).HasPrecision(10, 2);
                entity.Property(s => s.VatRate).HasPrecision(5, 4);
                entity.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(s => s.ReceiptNumber).IsUnique();
                entity.HasIndex(s => s.IdempotencyKey).IsUnique();
                entity.HasIndex(s => s.CreatedAt);
                entity.HasMany(s => s.Lines)
                      .WithOne()
                      .HasForeignKey(sl => sl.SaleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SaleLine>(entity =>
            {
                entity.Property(sl => sl.UnitPrice).HasPrecision(10, 2);
                entity.Property(sl => sl.LineTotal).HasPrecision(10, 2);
                entity.Property(sl => sl.ProductName).HasMaxLength(200);
                entity.HasOne<Product>()
                      .WithMany()
                      .HasForeignKey(sl => sl.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<DateTime>()
                .HaveConversion<UtcDateTimeConverter>();
        }
    }
}
