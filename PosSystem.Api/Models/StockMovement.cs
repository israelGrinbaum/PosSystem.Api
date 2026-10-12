namespace PosSystem.Api.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public StockMovementType Type { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SaleId { get; set; }
        public Sale? Sale { get; set; }
    }
}
