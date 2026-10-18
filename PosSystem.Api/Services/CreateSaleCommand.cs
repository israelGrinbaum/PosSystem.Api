using PosSystem.Api.Models;

namespace PosSystem.Api.Services
{
    public record CreateSaleCommand(
        IReadOnlyList<SaleLineInput> Lines,
        PaymentMethod PaymentMethod,
        decimal? AmountPaid,
        Guid IdempotencyKey);
}
