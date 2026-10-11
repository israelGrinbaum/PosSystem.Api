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
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            return await db.Products.Where(p => p.IsActive).ToListAsync();
        }
        [HttpPost]
        public async Task<ActionResult<Product>> Create(CreateProductRequest request)
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
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await db.Products.FindAsync(id);
            if(product is null) return NotFound();
            return product;
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Product>> Update (int id, UpdateProductRequest request)
        {
            if (await db.Products.AnyAsync(p => p.Sku == request.Sku & p.Id!=id))
            {
                return Problem(
                    detail: $"Product with SKU '{request.Sku}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            var product = await db.Products.FindAsync(id);
            if (product is null) return NotFound();
            product.Sku = request.Sku;
            product.Name = request.Name;
            product.Price = request.Price;
            await db.SaveChangesAsync();
            return product;
        }
    }
}
