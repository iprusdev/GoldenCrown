namespace GoldenCrown.Services
{
    public interface IFinanceService
    {
        Task<Result<decimal>> GetBalanceAsync(string token);
    }
}
