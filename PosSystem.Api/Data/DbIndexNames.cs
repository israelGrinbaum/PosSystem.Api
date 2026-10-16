namespace PosSystem.Api.Data
{
    public static class DbIndexNames
    {
        public const string ProductSku = "IX_Products_Sku";
        public const string SaleIdempotencyKey = "IX_Sales_IdempotencyKey";
        public const string SaleReceiptNumber = "IX_Sales_ReceiptNumber";
    }
}
