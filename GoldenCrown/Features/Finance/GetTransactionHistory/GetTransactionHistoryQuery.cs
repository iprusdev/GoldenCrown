using GoldenCrown.DTOs.Finance;
using MediatR;

namespace GoldenCrown.Features.Finance.GetTransactionHistory;

public sealed record GetTransactionHistoryQuery(int UserId, DateTimeOffset? From, DateTimeOffset? To, int Offset, int Limit) : IRequest<Result<IEnumerable<TransactionHistoryResponse>>>;
