using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Tests.Infrastructure;

namespace PosSystem.Api.Tests.Services
{
    [Collection("Database")]
    public class SaleServiceTests(DatabaseFixture fixture) : IAsyncLifetime
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
        public async Task Database_StartsEmpty()
        {
            using var db = fixture.CreateContext();
            Assert.False(await db.Products.AnyAsync());
        }
    }
}
