namespace PosSystem.Api.Dtos
{
    public record ProductDetailsResponse(int Id, string Sku, string Name, decimal Price, bool IsActive, DateTime CreatedAt, int CurrentStock);
}
