using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosSystem.Api.Data;
using PosSystem.Api.Models;
using PosSystem.Api.Services;
using PosSystem.Api.Settings;
using PosSystem.Api.Tests.Infrastructure;

namespace PosSystem.Api.Tests.Services
{
    [Collection("Database")]
    public class SaleServiceTests(DatabaseFixture fixture) : IAsyncLifetime
    {
        private const decimal VatRate = 0.18m;

        public async Task InitializeAsync()
        {
            await fixture.ResetAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CreateSaleAsync_InsufficientStock_ReturnsConflictAndSavesNothing()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 6.90m, stock: 4);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 5)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: null,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Conflict, result.Status);
            Assert.Contains("requested 5, available 4", result.ErrorDetail);

            await using var assertDb = fixture.CreateContext();
            Assert.False(await assertDb.Sales.AnyAsync());
            Assert.Equal(1, await assertDb.StockMovements.CountAsync());
        }

        [Fact]
        public async Task CreateSaleAsync_DuplicateLinesExceedStock_ReturnsConflict()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 6.90m, stock: 4);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 3), new SaleLineInput(productId, 3)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: null,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Conflict, result.Status);
            Assert.Contains("requested 6, available 4", result.ErrorDetail);

            await using var assertDb = fixture.CreateContext();
            Assert.False(await assertDb.Sales.AnyAsync());
            Assert.Equal(1, await assertDb.StockMovements.CountAsync());
        }

        [Fact]
        public async Task CreateSaleAsync_QuantityEqualsStock_CreatesSale()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 6.90m, stock: 4);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 4)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: null,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Created, result.Status);

            await using var assertDb = fixture.CreateContext();
            Assert.Equal(1, await assertDb.Sales.CountAsync());
            Assert.Equal(0, await assertDb.StockMovements
                .Where(sm => sm.ProductId == productId)
                .SumAsync(sm => sm.Quantity));
        }

        [Fact]
        public async Task CreateSaleAsync_CardSale_ExtractsVatAndIgnoresAmountPaid()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 118.00m, stock: 1);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 1)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: 500m,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Created, result.Status);

            await using var assertDb = fixture.CreateContext();
            var sale = await assertDb.Sales.SingleAsync();

            Assert.Equal(118.00m, sale.Total);
            Assert.Equal(18.00m, sale.VatAmount);
            Assert.Equal(0.18m, sale.VatRate);
            Assert.Equal(118.00m, sale.AmountPaid);
            Assert.Equal(0.00m, sale.Change);
        }

        [Fact]
        public async Task CreateSaleAsync_VatWithRepeatingDecimal_RoundsToAgorot()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 10.00m, stock: 1);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 1)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: null,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Created, result.Status);

            await using var assertDb = fixture.CreateContext();
            var sale = await assertDb.Sales.SingleAsync();

            Assert.Equal(1.53m, sale.VatAmount);
        }

        [Fact]
        public async Task CreateSaleAsync_VatAtMidpoint_RoundsAwayFromZero()
        {
            // Arrange
            var productId = await SeedProductAsync("Milk", 0.12m, stock: 1);

            var command = new CreateSaleCommand(
                Lines: [new SaleLineInput(productId, 1)],
                PaymentMethod: PaymentMethod.Card,
                AmountPaid: null,
                IdempotencyKey: Guid.NewGuid());

            // Act
            await using var actDb = fixture.CreateContext();
            var result = await CreateService(actDb, vatRate: 0.6m).CreateSaleAsync(command);

            // Assert
            Assert.Equal(CreateSaleStatus.Created, result.Status);

            await using var assertDb = fixture.CreateContext();
            var sale = await assertDb.Sales.SingleAsync();

            Assert.Equal(0.05m, sale.VatAmount);
        }

        private async Task<int> SeedProductAsync(string name, decimal price, int stock)
        {
            await using var db = fixture.CreateContext();
            var product = new Product
            {
                Name = name,
                Price = price,
                Sku = $"SKU-{Guid.NewGuid():N}",
                CreatedAt = DateTime.UtcNow,
            };
            db.Products.Add(product);
            if (stock > 0)
            {
                db.StockMovements.Add(new StockMovement
                {
                    Product = product,
                    Quantity = stock,
                    Type = StockMovementType.Receipt,
                    CreatedAt = DateTime.UtcNow,
                });
            }
            await db.SaveChangesAsync();
            return product.Id;
        }

        private static SaleService CreateService(PosDbContext db, decimal vatRate = VatRate) =>
            new(db, Options.Create(new VatOptions { Rate = vatRate }));
    }
}
