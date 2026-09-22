using GoldenCrown.Models;
using MediatR;

namespace GoldenCrown.Features.Finance.Transfer;

public sealed record TransferCommand(int FromUserId, string ReceiverLogin, decimal Amount, Currency Currency) : IRequest<Result>;
