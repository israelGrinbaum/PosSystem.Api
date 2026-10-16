using Microsoft.AspNetCore.Mvc;
using PosSystem.Api.Data;
using PosSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using PosSystem.Api.Dtos;

namespace PosSystem.Api.Controllers
{
    [ApiController]
    [Route("api/products")]

    public class ProductsController(PosDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> GetAll(string? search)
        {
            var query = db.Products.Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search.Trim()) || p.Sku.Contains(search.Trim()));
            }
            return await query.OrderBy(p => p.Name).Select(p => ProductResponse.FromEntity(p)).ToListAsync();
        }
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
        {
            if (await db.Products.AnyAsync(p => p.Sku == request.Sku))
            {
                return Problem(
                    detail: $"Product with SKU '{request.Sku}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var product = new Product
            {
                Sku = request.Sku,
                Name = request.Name,
                Price = request.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Products.Add(product);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation(DbIndexNames.ProductSku))
            {
                return Problem(
                    detail: $"Product with SKU '{request.Sku}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            var response = ProductResponse.FromEntity(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDetailsResponse>> GetById(int id)
        {
            var product = await db.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailsResponse(
                    p.Id,
                    p.Sku,
                    p.Name,
                    p.Price,
                    p.IsActive,
                    p.CreatedAt,
                    db.StockMovements
                        .Where(m => m.ProductId == p.Id)
                        .Sum(m => m.Quantity)))
                .FirstOrDefaultAsync();
            if (product is null) return NotFound();
            return product;
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();
            if (await db.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id))
            {
                return Problem(
                    detail: $"Product with SKU '{request.Sku}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            product.Sku = request.Sku;
            product.Name = request.Name;
            product.Price = request.Price;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation(DbIndexNames.ProductSku))
            {
                return Problem(
                    detail: $"Product with SKU '{request.Sku}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            var response = ProductResponse.FromEntity(product);
            return response;
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();
            product.IsActive = false;
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
