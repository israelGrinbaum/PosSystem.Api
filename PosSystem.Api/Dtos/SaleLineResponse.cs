using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record SaleLineResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal)
    {
        public static SaleLineResponse FromEntity(SaleLine saleLine) =>
            new(saleLine.ProductId,
                saleLine.ProductName,
                saleLine.UnitPrice,
                saleLine.Quantity,
                saleLine.LineTotal);
    }
}
