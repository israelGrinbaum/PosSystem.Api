using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record StockMovementResponse(int Id, int ProductId, int Quantity, StockMovementType Type, string? Reason, DateTime CreatedAt);
}
