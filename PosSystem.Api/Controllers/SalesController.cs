using Microsoft.AspNetCore.Mvc;
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
            if(request.IdempotencyKey == Guid.Empty)
            {
                ModelState.AddModelError(nameof(request.IdempotencyKey), "Idempotency key is required.");
            }
            if(request.PaymentMethod == PaymentMethod.Cash && request.AmountPaid is null)
            {
                ModelState.AddModelError(nameof(request.AmountPaid), "AmountPaid is required for cash payments.");
            }
            if(!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            return Problem(detail: "Not implemented yet", statusCode: 501);
        }
    }
}
