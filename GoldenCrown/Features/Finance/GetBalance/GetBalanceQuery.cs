using GoldenCrown.Models;
using MediatR;

namespace GoldenCrown.Features.Finance.GetBalance;

public sealed record GetBalanceQuery(int UserId, Currency Currency) : IRequest<Result<decimal>>;
