using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record StockMovementResponse(int Id, int ProductId, int Quantity, StockMovementType Type, string? Reason, DateTime CreatedAt)
    {
        public static StockMovementResponse FromEntity(StockMovement movement) =>
        new(movement.Id,
            movement.ProductId,
            movement.Quantity,
            movement.Type,
            movement.Reason,
            movement.CreatedAt);
    }
}
