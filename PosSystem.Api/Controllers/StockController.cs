using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Data;
using PosSystem.Api.Dtos;
using PosSystem.Api.Models;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController(PosDbContext db) : ControllerBase
    {
        [HttpPost("receipts")]
        public async Task<ActionResult<StockMovement>> ReceiveStock(CreateStockReceiptRequest request)
        {
            var product = await db.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound();
            if (!product.IsActive)
            {
                return Problem(
                    detail: $"Product '{request.ProductId}' is not active.",
                    statusCode: StatusCodes.Status409Conflict
                );
            }
            StockMovement stockMovement = new StockMovement
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Type = StockMovementType.Receipt,
                CreatedAt = DateTime.UtcNow
            };
            db.StockMovements.Add(stockMovement);
            await db.SaveChangesAsync();
            return Created($"/api/products/{stockMovement.Id}", stockMovement);
        }
    }
}
