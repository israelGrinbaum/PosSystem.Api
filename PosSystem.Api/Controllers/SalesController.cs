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

            return Problem(detail: "Not implemented yet", statusCode: 501);
        }
    }
}
