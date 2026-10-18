namespace PosSystem.Api.Services
{
    public enum CreateSaleStatus
    {
        Created,
        AlreadyExists,
        NotFound,
        Conflict,
        Validation
    }
}
