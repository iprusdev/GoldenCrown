using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountModel = GoldenCrown.Models.Account;

namespace GoldenCrown.Features.Account.CreateAccount;

public sealed class CreateAccountCommandHandler(ApplicationDbContext context)
    : IRequestHandler<CreateAccountCommand>
{
    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Currency))
        {
            throw new ArgumentException("Укажите валюту USD, EUR или BYN");
        }

        var user = await context.Users.FirstOrDefaultAsync(user => user.Login == request.Login, cancellationToken)
            ?? throw new InvalidOperationException($"Unable to find user with login {request.Login}");

        if (await context.Accounts.AnyAsync(account => account.UserId == user.Id && account.Currency == request.Currency, cancellationToken))
        {
            throw new InvalidOperationException($"Account already exists for user with login {request.Login}");
        }

        context.Accounts.Add(new AccountModel { UserId = user.Id, Currency = request.Currency, Balance = 0 });
        await context.SaveChangesAsync(cancellationToken);
    }
}
