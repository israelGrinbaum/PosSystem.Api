using Microsoft.AspNetCore.Mvc;
using PosSystem.Api.Data;
using PosSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

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

    }
}
