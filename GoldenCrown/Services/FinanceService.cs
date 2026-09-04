using GoldenCrown.Data;
using GoldenCrown.DTOs.Finance;
using GoldenCrown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace GoldenCrown.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly ApplicationDbContext _context;
        public FinanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<decimal>> GetBalanceAsync(int userId)
        {

            var account = await _context.Accounts
        .FirstOrDefaultAsync(a => userId == a.UserId);
            if (account == null)
            {
                return Result<decimal>.Failure("Account not found");
            }
            return Result<decimal>.Success(account.Balance);
        }
        public async Task<Result> DepositAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                return Result.Failure("Сумма должна быть больше нуля");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(b => b.UserId == userId);
            if (account == null)
            {
                return Result.Failure("Счёт не найден");
            }

            account.Balance += amount;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> TransferAsync(int fromUserId, string receiverLogin, decimal amount)
        {
            if (amount <= 0)
            {
                return Result.Failure("Сумма должна быть больше нуля");
            }

            var fromSender = await _context.Users.FirstOrDefaultAsync(a => a.Id == fromUserId);
            if (fromSender == null)
            {
                return Result.Failure("Отправитель не найден");
            }

            var toSender = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == fromSender.Id);
            if (toSender == null)
            {
                return Result.Failure("Счёт отправителя не найден");
            }

            var userReceiver = await _context.Users.FirstOrDefaultAsync(a => a.Login == receiverLogin);
            if (userReceiver == null)
            {
                return Result.Failure("Получатель не найден");
            }

            if (userReceiver.Id == fromUserId)
            {
                return Result.Failure("Нельзя перевести средства самому себе");
            }

            var toReceiver = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userReceiver.Id);
            if (toReceiver == null)
            {
                return Result.Failure("Счёт получателя не найден");
            }

            if (toSender.Balance < amount)
            {
                return Result.Failure("Недостаточно средств");
            }

            toSender.Balance -= amount;
            toReceiver.Balance += amount;
            var transaction = new GoldenCrown.Models.Transaction
            {
                SenderId = toSender.UserId,
                ReceiverId = toReceiver.UserId,
                Amount = amount,
                Date = DateTimeOffset.UtcNow
            };
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result<IEnumerable<TransactionHistoryResponse>>> GetHistoryAsync(int userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int skip, int take)
        {
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            {
                return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Некорректный диапазон дат");
            }
            if (skip < 0)
            {
                return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Offset не может быть отрицательным");
            }
            if (take <= 0)
            {
                return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Limit должен быть больше нуля");
            }


            var transactions = _context.Transactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.SenderId == userId ||
                    transaction.ReceiverId == userId);

            if (dateFrom.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Date >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Date <= dateTo.Value);
            }

            var result = await transactions
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Id)
                .Skip(skip)
                .Take(take)
                .Select(transaction => new TransactionHistoryResponse
                {
                    SenderName = transaction.Sender == null ? null : transaction.Sender.Name,
                    ReceiverName = transaction.Receiver.Name,
                    Amount = transaction.Amount,
                    Date = transaction.Date
                })
                .ToListAsync();

            return Result<IEnumerable<TransactionHistoryResponse>>.Success(result);
        }
    }
}
