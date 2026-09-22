using GoldenCrown.Models;
using GoldenCrown.DTOs.Finance;
using MediatR;

namespace GoldenCrown.Features.Finance.GetTransactionHistory;

public sealed record GetTransactionHistoryQuery(int UserId, DateTimeOffset? From, DateTimeOffset? To, int Offset, int Limit, Currency? Currency = null) : IRequest<Result<IEnumerable<TransactionHistoryResponse>>>;
