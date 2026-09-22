using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.GetBalance;

public sealed class GetBalanceQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetBalanceQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Currency))
        {
            return Result<decimal>.Failure("Укажите валюту USD, EUR или BYN");
        }

        var account = await context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => request.UserId == a.UserId && a.Currency == request.Currency, cancellationToken);
        if (account == null)
        {
            return Result<decimal>.Failure("Account not found");
        }
        return Result<decimal>.Success(account.Balance);
    }
}
