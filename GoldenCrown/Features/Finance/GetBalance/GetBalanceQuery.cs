using MediatR;

namespace GoldenCrown.Features.Finance.GetBalance;

public sealed record GetBalanceQuery(int UserId) : IRequest<Result<decimal>>;
