namespace PosSystem.Api.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int ReceiptNumber { get; set; }
        public Guid IdempotencyKey { get; set; }
        public decimal Total { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SaleLine> Lines { get; set; } = [];
    }
}
