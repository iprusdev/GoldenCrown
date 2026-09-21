using MediatR;

namespace GoldenCrown.Features.Finance.Deposit;

public sealed record DepositCommand(int UserId, decimal Amount) : IRequest<Result>;
