using PosSystem.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Dtos
{
    public class CreateSaleRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public List<CreateSaleLineRequest> Lines { get; set; } = [];

        [Required]
        [EnumDataType(typeof(PaymentMethod))]
        public PaymentMethod? PaymentMethod { get; set; }

        [Range(typeof(decimal), "0.01", "999999")]
        public decimal? AmountPaid { get; set; }

        [Required]
        public Guid? IdempotencyKey { get; set; }
    }
}
