using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Data;
using PosSystem.Api.Models;
using PosSystem.Api.Tests.Infrastructure;

namespace PosSystem.Api.Tests.Data
{
    [Collection("Database")]
    public class DbUpdateExceptionExtensionsTests(DatabaseFixture fixture) : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            await fixture.ResetAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task IdempotencyKeyIndex_DuplicateKey_IsDetectedAsUniqueViolation()
        {
            // Arrange
            var idempotencyKey = Guid.NewGuid();
            var firstSale = new Sale
            {
                ReceiptNumber = 1,
                IdempotencyKey = idempotencyKey,
            };

            await using var firstDb = fixture.CreateContext();
            firstDb.Sales.Add(firstSale);
            await firstDb.SaveChangesAsync();

            // Act
            var actSale = new Sale
            {
                ReceiptNumber = 2,
                IdempotencyKey = idempotencyKey,
            };

            await using var actDb = fixture.CreateContext();
            actDb.Sales.Add(actSale);
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => actDb.SaveChangesAsync());

            // Assert
            Assert.True(ex.IsUniqueViolation(DbIndexNames.SaleIdempotencyKey));
            Assert.False(ex.IsUniqueViolation(DbIndexNames.SaleReceiptNumber));
        }
    }
}
