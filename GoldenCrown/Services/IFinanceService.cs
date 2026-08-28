using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.Services
{
    public interface IFinanceService
    {
        Task<Result> DepositAsync(string token, decimal amount);
        Task<Result<decimal>> GetBalanceAsync(string token);
        Task<Result<IEnumerable<TransactionHistoryResponse>>> GetHistoryAsync(string token, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int skip, int take);
        Task<Result> TransferAsync(string token, string receiverLogin, decimal amount);
    }
}
