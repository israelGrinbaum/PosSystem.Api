using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Dtos
{
    public class CreateStockAdjustmentRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(-100000, 100000)]
        public int Quantity { get; set; }
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
