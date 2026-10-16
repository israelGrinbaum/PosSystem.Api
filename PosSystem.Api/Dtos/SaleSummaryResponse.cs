using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record SaleSummaryResponse(
        int Id,
        int ReceiptNumber,
        DateTime CreatedAt,
        decimal Total,
        PaymentMethod PaymentMethod);
}
