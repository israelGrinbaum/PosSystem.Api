using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Data;
using PosSystem.Api.Dtos;
using PosSystem.Api.Models;
using PosSystem.Api.Services;
using System.Diagnostics;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController(PosDbContext db, ISaleService saleService) : ControllerBase
    {
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

            var command = new CreateSaleCommand(
                Lines: request.Lines.Select(line => new SaleLineInput(
                    ProductId: line.ProductId,
                    Quantity: line.Quantity)).ToList(),
                PaymentMethod: request.PaymentMethod!.Value,
                AmountPaid: request.AmountPaid,
                IdempotencyKey: request.IdempotencyKey!.Value);
            var result = await saleService.CreateSaleAsync(command);

            if (!result.IsSuccess)
            {
                return MapError(result);
            }
            if (result.Status == CreateSaleStatus.AlreadyExists)
            {
                return Ok(SaleResponse.FromEntity(result.Sale));
            }

            return CreatedAtAction(nameof(GetSale), new { id = result.Sale.Id }, SaleResponse.FromEntity(result.Sale));
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

        private ActionResult<SaleResponse> MapError(CreateSaleResult result)
        {
            if (result.IsSuccess)
            {
                throw new UnreachableException();
            }
            switch (result.Status)
            {
                case CreateSaleStatus.NotFound:
                    return Problem(
                        detail: result.ErrorDetail,
                        statusCode: StatusCodes.Status404NotFound);
                case CreateSaleStatus.Conflict:
                    return Problem(
                        detail: result.ErrorDetail,
                        statusCode: StatusCodes.Status409Conflict);
                case CreateSaleStatus.Validation:
                    if (result.ErrorField is not null)
                    {
                        ModelState.AddModelError(result.ErrorField, result.ErrorDetail);
                        return ValidationProblem(ModelState);
                    }
                    return Problem(
                        detail: result.ErrorDetail,
                        statusCode: StatusCodes.Status400BadRequest);
                default:
                    throw new UnreachableException();
            }
        }
    }
}