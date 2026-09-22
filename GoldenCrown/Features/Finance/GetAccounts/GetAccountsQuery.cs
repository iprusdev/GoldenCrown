using GoldenCrown.DTOs.Finance;
using MediatR;

namespace GoldenCrown.Features.Finance.GetAccounts;

public sealed record GetAccountsQuery(int UserId) : IRequest<IReadOnlyList<AccountResponse>>;
