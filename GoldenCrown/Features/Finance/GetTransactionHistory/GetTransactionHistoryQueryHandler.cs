using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.Features.Finance.GetTransactionHistory;

public sealed class GetTransactionHistoryQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetTransactionHistoryQuery, Result<IEnumerable<TransactionHistoryResponse>>>
{
    public async Task<Result<IEnumerable<TransactionHistoryResponse>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        if (request.Currency.HasValue && !Enum.IsDefined(request.Currency.Value))
            return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Укажите валюту USD, EUR или BYN");

        if (request.From.HasValue && request.To.HasValue && request.From > request.To)
        {
            return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Некорректный диапазон дат");
        }
        if (request.Offset < 0)
        {
            return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Offset не может быть отрицательным");
        }
        if (request.Limit <= 0)
        {
            return Result<IEnumerable<TransactionHistoryResponse>>.Failure("Limit должен быть больше нуля");
        }


        var transactions = context.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.SenderId == request.UserId ||
                transaction.ReceiverId == request.UserId);

        if (request.From.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.Date >= request.From.Value);
        }
        if (request.To.HasValue)
        {
            transactions = transactions.Where(transaction => transaction.Date <= request.To.Value);
        }

        if (request.Currency.HasValue)
            transactions = transactions.Where(transaction => transaction.Currency == request.Currency.Value);

        var result = await transactions
            .OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Id)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(transaction => new TransactionHistoryResponse
            {
                SenderName = transaction.Sender == null ? null : transaction.Sender.Name,
                ReceiverName = transaction.Receiver.Name,
                Amount = transaction.Amount, Currency = transaction.Currency,
                Date = transaction.Date
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<TransactionHistoryResponse>>.Success(result);
    }
}
