using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.Services
{
    public interface IFinanceService
    {
        Task<Result> DepositAsync(int userId, decimal amount);
        Task<Result<decimal>> GetBalanceAsync(int userId);
        Task<Result<IEnumerable<TransactionHistoryResponse>>> GetHistoryAsync(int userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int skip, int take);
        Task<Result> TransferAsync(int fromUserId, string receiverLogin, decimal amount);
    }
}
