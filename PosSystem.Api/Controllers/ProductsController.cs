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
            return Created($"/api/products/{product.Id}", product);
        }
    }
}
