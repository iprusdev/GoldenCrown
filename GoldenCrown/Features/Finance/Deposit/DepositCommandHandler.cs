using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.Deposit;

public sealed class DepositCommandHandler(ApplicationDbContext context)
    : IRequestHandler<DepositCommand, Result>
{
    public async Task<Result> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Result.Failure("Сумма должна быть больше нуля");
        }

        var account = await context.Accounts.FirstOrDefaultAsync(b => b.UserId == request.UserId, cancellationToken);
        if (account == null)
        {
            return Result.Failure("Счёт не найден");
        }

        account.Balance += request.Amount;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
