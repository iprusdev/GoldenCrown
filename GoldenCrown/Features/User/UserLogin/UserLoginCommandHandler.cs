using GoldenCrown.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserModel = GoldenCrown.Models.User;

namespace GoldenCrown.Features.User.UserLogin;

public sealed class UserLoginCommandHandler(ApplicationDbContext context, IPasswordHasher<UserModel> passwordHasher)
    : IRequestHandler<UserLoginCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.Users.FirstOrDefaultAsync(u => u.Login == request.Login, cancellationToken);
        if (existing == null)
        {
            return Result<string>.Failure("Invalid login or password");
        }
        var passwordVerification = VerifyPassword(existing, request.Password);
        if (passwordVerification == PasswordVerificationResult.Failed)
        {
            return Result<string>.Failure("Invalid login or password");
        }

        if (passwordVerification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            existing.PasswordHash = passwordHasher.HashPassword(existing, request.Password);
        }
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.UserId == existing.Id, cancellationToken);
        if (session == null)
        {
            session = new GoldenCrown.Models.Session { UserId = existing.Id };
            context.Sessions.Add(session);
        }

        session.Token = Guid.NewGuid().ToString();
        session.ExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(session.Token);
    }

    private PasswordVerificationResult VerifyPassword(UserModel user, string password)
    {
        try
        {
            var result = passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            if (result != PasswordVerificationResult.Failed)
            {
                return result;
            }
        }
        catch (FormatException)
        {
            // Older application versions stored passwords without hashing.
        }

        if (user.PasswordHash != password)
        {
            return PasswordVerificationResult.Failed;
        }

        return PasswordVerificationResult.SuccessRehashNeeded;
    }
}
