using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosSystem.Api.Data;
using PosSystem.Api.Dtos;
using PosSystem.Api.Models;
using PosSystem.Api.Settings;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController(PosDbContext db, IOptions<VatOptions> vatOptions) : ControllerBase
    {
        private readonly decimal _vatRate = vatOptions.Value.Rate;

        // Matches HasPrecision(10,2) on Sale.Total. Update both together.
        private const decimal MaxSaleTotal = 99_999_999.99m;

        [HttpPost]
        public async Task<ActionResult<SaleResponse>> CreateSale(CreateSaleRequest request)
        {
            if (request.IdempotencyKey == Guid.Empty)
            {
                ModelState.AddModelError(nameof(request.IdempotencyKey), "Idempotency key is required.");
            }
            if (request.PaymentMethod == PaymentMethod.Cash && request.AmountPaid is null)
            {
                ModelState.AddModelError(nameof(request.AmountPaid), "AmountPaid is required for cash payments.");
            }
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var idempotencyKey = request.IdempotencyKey!.Value;
            var existingSale = await db.Sales.AsNoTracking()
                .Include(s => s.Lines.OrderBy(sl => sl.Id))
                .SingleOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);
            if (existingSale is not null)
            {
                return Ok(SaleResponse.FromEntity(existingSale));
            }

            var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToList();
            var products = await db.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);
            var missingIds = productIds
                .Where(id => !products.ContainsKey(id)).ToList();
            if (missingIds.Count > 0)
            {
                return Problem(
                    detail: $"Products not found: {string.Join(", ", missingIds)}.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            var inactiveNames = products.Values
                .Where(p => !p.IsActive)
                .Select(p => p.Name)
                .ToList();
            if (inactiveNames.Count > 0)
            {
                return Problem(
                    detail: $"Products not active: {string.Join(", ", inactiveNames)}.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var requestedQuantities = request.Lines
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
                return Problem(
                    detail: $"Insufficient stock: {string.Join(", ", shortages)}.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var saleLines = new List<SaleLine>();
            foreach (var line in request.Lines)
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
                return Problem(
                    detail: $"Sale total exceeds maximum allowed value of {MaxSaleTotal}.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var vatAmount = Math.Round(total * _vatRate / (1 + _vatRate), 2, MidpointRounding.AwayFromZero);

            var amountPaid = request.PaymentMethod == PaymentMethod.Cash ? request.AmountPaid!.Value : total;

            if (request.PaymentMethod == PaymentMethod.Cash && amountPaid < total)
            {
                ModelState.AddModelError(nameof(request.AmountPaid), $"Amount paid ({amountPaid}) is less than total sale amount ({total}).");
                return ValidationProblem(ModelState);
            }

            var change = amountPaid - total;

            var lastReceiptNumber = await db.Sales.MaxAsync(s => (int?)s.ReceiptNumber) ?? 0;

            var now = DateTime.UtcNow;

            var sale = new Sale
            {
                AmountPaid = amountPaid,
                ReceiptNumber = lastReceiptNumber + 1,
                PaymentMethod = request.PaymentMethod!.Value,
                VatRate = _vatRate,
                VatAmount = vatAmount,
                Change = change,
                Total = total,
                Lines = saleLines,
                CreatedAt = now,
                IdempotencyKey = idempotencyKey,
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
                    CreatedAt = now
                });
            }

            db.Sales.Add(sale);
            db.StockMovements.AddRange(stockMovements);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, SaleResponse.FromEntity(sale));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SaleResponse>> GetSale(int id)
        {
            var sale = await db.Sales.AsNoTracking()
                .Include(s => s.Lines.OrderBy(sl => sl.Id))
                .SingleOrDefaultAsync(s => s.Id == id);

            if (sale is null)
            {
                return NotFound();
            }

            return Ok(SaleResponse.FromEntity(sale));
        }

        [HttpGet]
        public async Task<ActionResult<List<SaleSummaryResponse>>> GetSales(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            if (!from.HasValue)
            {
                ModelState.AddModelError(nameof(from), "From date is required.");
            }
            if (!to.HasValue)
            {
                ModelState.AddModelError(nameof(to), "To date is required.");
            }
            if (from >= to)
            {
                ModelState.AddModelError(nameof(from), "From date must be earlier than To date.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var fromUtc = from!.Value.UtcDateTime;
            var toUtc = to!.Value.UtcDateTime;

            var sales = await db.Sales.AsNoTracking()
                .Where(s => s.CreatedAt >= fromUtc && s.CreatedAt < toUtc)
                .OrderByDescending(s => s.CreatedAt)
                .ThenByDescending(s => s.Id)
                .Select(s => new SaleSummaryResponse(
                    Id: s.Id,
                    ReceiptNumber: s.ReceiptNumber,
                    CreatedAt: s.CreatedAt,
                    Total: s.Total,
                    PaymentMethod: s.PaymentMethod))
                .ToListAsync();

            return Ok(sales);
        }
    }
}
