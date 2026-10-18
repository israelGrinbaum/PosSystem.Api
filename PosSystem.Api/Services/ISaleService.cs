namespace PosSystem.Api.Services
{
    public interface ISaleService
    {
        Task<CreateSaleResult> CreateSaleAsync(CreateSaleCommand command);
    }
}
