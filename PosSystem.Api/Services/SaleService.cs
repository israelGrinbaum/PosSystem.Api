using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosSystem.Api.Data;
using PosSystem.Api.Models;
using PosSystem.Api.Settings;

namespace PosSystem.Api.Services
{
    public class SaleService(PosDbContext db, IOptions<VatOptions> vatOptions) : ISaleService
    {
        // Matches HasPrecision(10,2) on Sale.Total. Update both together.
        private const decimal MaxSaleTotal = 99_999_999.99m;

        private readonly decimal _vatRate = vatOptions.Value.Rate;

        public async Task<CreateSaleResult> CreateSaleAsync(CreateSaleCommand command)
        {
            if (command.PaymentMethod == PaymentMethod.Cash && command.AmountPaid is null)
            {
                return CreateSaleResult.Validation(
                    "AmountPaid is required for cash payments.",
                    nameof(command.AmountPaid));
            }

            var existingSale = await FindByIdempotencyKeyAsync(command.IdempotencyKey);
            if (existingSale is not null)
            {
                return CreateSaleResult.AlreadyExists(existingSale);
            }

            var productIds = command.Lines
                .Select(line => line.ProductId)
                .Distinct()
                .ToList();
            var products = await db.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);
            var missingIds = productIds
                .Where(id => !products
                .ContainsKey(id))
                .ToList();
            if (missingIds.Count > 0)
            {
                return CreateSaleResult.NotFound($"Products not found: {string.Join(", ", missingIds)}.");
            }

            var inactiveNames = products.Values
                .Where(p => !p.IsActive)
                .Select(p => p.Name)
                .ToList();
            if (inactiveNames.Count > 0)
            {
                return CreateSaleResult.Conflict($"Products not active: {string.Join(", ", inactiveNames)}.");
            }

            var requestedQuantities = command.Lines
                .GroupBy(line => line.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(line => line.Quantity));

            var currentStock = await db.StockMovements
                .Where(sm => productIds.Contains(sm.ProductId))
                .GroupBy(sm => sm.ProductId)
                .Select(g => new { ProductId = g.Key, Stock = g.Sum(sm => sm.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Stock);

            var shortages = new List<string>();
            foreach (var (productId, requested) in requestedQuantities)
            {
                var available = currentStock.GetValueOrDefault(productId);
                if (available < requested)
                {
                    shortages.Add($"{products[productId].Name} (requested {requested}, available {available})");
                }
            }
            if (shortages.Count > 0)
            {
                return CreateSaleResult.Conflict($"Insufficient stock: {string.Join(", ", shortages)}.");
            }

            var saleLines = new List<SaleLine>();
            foreach (var line in command.Lines)
            {
                var product = products[line.ProductId];
                saleLines.Add(new SaleLine
                {
                    ProductId = line.ProductId,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = product.Price * line.Quantity
                });
            }

            var total = saleLines.Sum(sl => sl.LineTotal);
            if (total > MaxSaleTotal)
            {
                return CreateSaleResult.Validation(
                    $"Sale total exceeds maximum allowed value of {MaxSaleTotal}.");
            }

            var vatAmount = Math.Round(total * _vatRate / (1 + _vatRate), 2, MidpointRounding.AwayFromZero);

            var amountPaid = command.PaymentMethod == PaymentMethod.Cash ? command.AmountPaid!.Value : total;

            if (command.PaymentMethod == PaymentMethod.Cash && amountPaid < total)
            {
                return CreateSaleResult.Validation(
                    $"Amount paid ({amountPaid}) is less than total sale amount ({total}).",
                    nameof(command.AmountPaid));
            }

            var change = amountPaid - total;

            var lastReceiptNumber = await db.Sales.MaxAsync(s => (int?)s.ReceiptNumber) ?? 0;

            var now = DateTime.UtcNow;

            var sale = new Sale
            {
                ReceiptNumber = lastReceiptNumber + 1,
                IdempotencyKey = command.IdempotencyKey,
                Total = total,
                VatRate = _vatRate,
                VatAmount = vatAmount,
                PaymentMethod = command.PaymentMethod,
                AmountPaid = amountPaid,
                Change = change,
                CreatedAt = now,
                Lines = saleLines,
            };

            var stockMovements = new List<StockMovement>();
            foreach (var sl in saleLines)
            {
                stockMovements.Add(new StockMovement
                {
                    ProductId = sl.ProductId,
                    Quantity = -sl.Quantity,
                    Type = StockMovementType.Sale,
                    Sale = sale,
                    CreatedAt = now,
                });
            }

            db.Sales.Add(sale);
            db.StockMovements.AddRange(stockMovements);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation(DbIndexNames.SaleIdempotencyKey))
            {
                var concurrentSale = await FindByIdempotencyKeyAsync(command.IdempotencyKey);
                if (concurrentSale is null)
                {
                    return CreateSaleResult.Conflict("A concurrent sale with this key could not be loaded. Please retry with the same idempotency key.");
                }
                return CreateSaleResult.AlreadyExists(concurrentSale);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation(DbIndexNames.SaleReceiptNumber))
            {
                return CreateSaleResult.Conflict("A concurrent sale took the same receipt number. Please retry with the same idempotency key.");
            }

            return CreateSaleResult.Created(sale);
        }

        private Task<Sale?> FindByIdempotencyKeyAsync(Guid idempotencyKey) =>
            db.Sales.AsNoTracking()
                .Include(s => s.Lines.OrderBy(sl => sl.Id))
                .SingleOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);
    }
}
