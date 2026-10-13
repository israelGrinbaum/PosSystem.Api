using System.ComponentModel.DataAnnotations;

namespace PosSystem.Api.Dtos
{
    public class CreateSaleLineRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; }
    }
}
