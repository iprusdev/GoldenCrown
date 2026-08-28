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

        public async Task<Result<decimal>> GetBalanceAsync(string token)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session == null)
            {
                return Result<decimal>.Failure("Session not found");
            }
            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return Result<decimal>.Failure("Session expired");
            }
            var account = await _context.Accounts
        .FirstOrDefaultAsync(a => session.UserId == a.UserId);
            if (account == null)
            {
                return Result<decimal>.Failure("Account not found");
            }
            return Result<decimal>.Success(account.Balance);
        }
        public async Task<Result> DepositAsync(string token, decimal amount)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session == null)
            {
                return Result.Failure("Session not found");
            }
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Id == session.UserId);
            var account = await _context.Accounts.FirstOrDefaultAsync(b => b.UserId == user!.Id);

            account!.Balance += amount;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> TransferAsync(string token, string receiverLogin, decimal amount)
        {
            var session =await _context.Sessions.FirstOrDefaultAsync(s=>s.Token == token);
            if (session == null)
            {
                return Result.Failure("Session not found");
            }
            //
            var fromSender = await _context.Users.FirstOrDefaultAsync(a => a.Id == session.UserId);
            var toSender = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == fromSender!.Id);
            //
            var userReceiver = await _context.Users.FirstOrDefaultAsync(a => a!.Login == receiverLogin);
            if (userReceiver == null)
            {
                return Result.Failure("Получатель не найден");
            }
            var toReceiver = await _context.Accounts.FirstOrDefaultAsync(a => a.User == userReceiver);
            if (toSender.Balance < amount)
            {
                return Result.Failure("Недостаточно средств");
            }
            if (toSender.Balance>0 && toSender.Balance > amount)
            {
                toSender.Balance -= amount;
                toReceiver.Balance += amount;
            }
            var transaction = new GoldenCrown.Models.Transaction
            {
                SenderId = toSender.UserId,
                ReceiverId = toReceiver.UserId,
                Amount = amount,
                Date = DateTime.Now
            };
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result<IEnumerable<TransactionHistoryResponse>>> GetHistoryAsync(string token, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int skip, int take)
        {
            //var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            //if (session == null) return Result<IEnumerable<TransactionHistoryResponce>>.Failure("User not found");

            //var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == session!.UserId);

            //var transactionsQuery = _context.Transactions
            //    .Where(t => t.SenderId == account.Id || t.ReceiverId == account.Id)
            //    .OrderByDescending(t => t.Date)
            //    .AsQueryable();

            //if (dateFrom != null)
            //{
            //    transactionsQuery = transactionsQuery.Where(t => t.Date >= dateFrom.Value);
            //}

            //if (dateTo != null)
            //{
            //    transactionsQuery = transactionsQuery.Where(t => t.Date <= dateTo.Value);
            //}

            //var transactions = await transactionsQuery
            //    .Skip(skip)
            //    .Take(take)
            //    .ToListAsync();

            //var result = transactions.Select(t => new TransactionHistoryResponce
            //{ 
            //    SenderId = t.SenderId,
            //    ReceiverId = t.ReceiverId,
            //    Date = t.Date,
            //    Amount = t.Amount
            //});

            //return Result<IEnumerable<TransactionHistoryResponce>>.Success(result);

            if (dateFrom != null && dateTo != null && dateFrom > dateTo)
            {
                return Result<List<TransactionHistoryResponse>>.Failure("Некорректный диапазон дат");
            }

            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session == null)
            {
                return Result<List<TransactionHistoryResponse>>.Failure("Пользователь не авторизован");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == session.UserId);


            var transactions = _context.Transactions.Where(x => x.SenderId == account!.Id || x.ReceiverId == account.Id);

            if (dateFrom != null)
            {
                transactions = transactions.Where(x => x.Date >= dateFrom.Value);
            }
            if (dateTo != null)
            {
                transactions = transactions.Where(x => x.Date <= dateTo.Value);
            }
            transactions = transactions.Skip(skip).Take(take);

            var dbTransactions = await transactions.ToListAsync();

            var result = new List<TransactionHistoryResponse>();
            var allSenders = transactions.Select(x => x.SenderId);
            var allReceivers = transactions.Select(x => x.ReceiverId);
            var allAccounts = allSenders.ToHashSet();
            foreach (var receiver in allReceivers)
            {
                allAccounts.Add(receiver);
            }

            var names = await _context.Accounts.Where(x => allAccounts.Contains(x.Id))
                .Join(_context.Users,
                acc => acc.UserId,
                u => u.Id,
                (acc, u) => new
                {
                    Name = u.Name,
                    AccId = acc.Id,
                }).ToDictionaryAsync(x => x.AccId);

            foreach (var transaction in transactions)
            {
                var senderName = names[transaction.SenderId].Name;
                var receiverName = names[transaction.ReceiverId].Name;
                result.Add(new TransactionHistoryResponse
                {
                    SenderName = senderName,
                    ReceiverName = receiverName,
                    Amount = transaction.Amount,
                    Date = transaction.Date
                });
            }

            return result;
        }
    }
}