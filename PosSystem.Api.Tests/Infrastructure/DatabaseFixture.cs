using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Data;

namespace PosSystem.Api.Tests.Infrastructure
{
    public class DatabaseFixture
    {
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=PosSystem_Tests;Trusted_Connection=True;";
        public DatabaseFixture()
        {
            using var db = CreateContext();
            db.Database.EnsureDeleted();
            db.Database.Migrate();
        }

        public PosDbContext CreateContext() => new (new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

        public async Task ResetAsync()
        {
            await using var db = CreateContext();
            await db.StockMovements.ExecuteDeleteAsync();
            await db.SaleLines.ExecuteDeleteAsync();
            await db.Sales.ExecuteDeleteAsync();
            await db.Products.ExecuteDeleteAsync();
        }
    }
}
