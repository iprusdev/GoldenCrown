using GoldenCrown.Models;
using MediatR;

namespace GoldenCrown.Features.Finance.Deposit;

public sealed record DepositCommand(int UserId, decimal Amount, Currency Currency) : IRequest<Result>;
