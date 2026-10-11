namespace PosSystem.Api.Dtos
{
    public record InventoryItemResponse(int ProductId, string Sku, string Name, int CurrentStock);
}
