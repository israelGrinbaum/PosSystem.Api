using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record ProductResponse(int Id, string Sku, string Name, decimal Price, bool IsActive, DateTime CreatedAt)
    {
        public static ProductResponse FromEntity(Product product) =>
            new(product.Id,
                product.Sku,
                product.Name,
                product.Price,
                product.IsActive,
                product.CreatedAt);
    }
}
