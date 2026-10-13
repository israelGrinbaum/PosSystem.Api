using PosSystem.Api.Models;

namespace PosSystem.Api.Dtos
{
    public record SaleResponse(int Id,
                                int ReceiptNumber,
                                DateTime CreatedAt,
                                List<SaleLineResponse> Lines,
                                decimal Total,
                                decimal TotalBeforeVat,
                                decimal VatRate,
                                decimal VatAmount,
                                PaymentMethod PaymentMethod,
                                decimal AmountPaid,
                                decimal Change)
    {
        public static SaleResponse FromEntity(Sale sale) =>
            new(Id: sale.Id,
                ReceiptNumber: sale.ReceiptNumber,
                CreatedAt: sale.CreatedAt,
                Lines: sale.Lines.Select(SaleLineResponse.FromEntity).ToList(),
                Total: sale.Total,
                TotalBeforeVat: sale.Total - sale.VatAmount,
                VatRate: sale.VatRate,
                VatAmount: sale.VatAmount,
                PaymentMethod: sale.PaymentMethod,
                AmountPaid: sale.AmountPaid,
                Change: sale.Change);
    }
}
