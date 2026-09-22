using GoldenCrown.Data;
using GoldenCrown.DTOs.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.GetAccounts;

public sealed class GetAccountsQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetAccountsQuery, IReadOnlyList<AccountResponse>>
{
    public async Task<IReadOnlyList<AccountResponse>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        => await context.Accounts.AsNoTracking()
            .Where(account => account.UserId == request.UserId)
            .OrderBy(account => account.Currency)
            .Select(account => new AccountResponse(account.Id, account.Currency, account.Balance))
            .ToListAsync(cancellationToken);
}
