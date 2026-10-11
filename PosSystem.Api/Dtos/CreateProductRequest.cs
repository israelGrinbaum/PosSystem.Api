using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Dtos
{
    public class CreateProductRequest
    {
        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }
    }
}
