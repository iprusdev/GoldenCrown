using GoldenCrown.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserModel = GoldenCrown.Models.User;
using AccountModel = GoldenCrown.Models.Account;

namespace GoldenCrown.Features.User.UserRegister;

public sealed class UserRegisterCommandHandler(ApplicationDbContext context, IPasswordHasher<UserModel> passwordHasher)
    : IRequestHandler<UserRegisterCommand, Result>
{
    public async Task<Result> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(user => user.Login == request.Login, cancellationToken))
        {
            return Result.Failure("Пользователь с таким логином уже существует");
        }

        var user = new UserModel
        {
            Login = request.Login,
            Name = request.Name,
            Account = new AccountModel { Balance = 0 }
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        // Persist the user and account together so registration cannot leave an orphan user.
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
