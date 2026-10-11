using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Dtos
{
    public class CreateStockReceiptRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, 100000, ErrorMessage = "Quantity must be a positive number.")]
        public int Quantity { get; set; }
    }
}
