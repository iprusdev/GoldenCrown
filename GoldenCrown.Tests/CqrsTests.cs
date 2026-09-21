using GoldenCrown.Data;
using GoldenCrown.Features.Account.CreateAccount;
using GoldenCrown.Features.Finance.Deposit;
using GoldenCrown.Features.Finance.GetBalance;
using GoldenCrown.Features.Finance.GetTransactionHistory;
using GoldenCrown.Features.Finance.Transfer;
using GoldenCrown.Features.Session.CleanupExpiredSessions;
using GoldenCrown.Features.User.UserLogin;
using GoldenCrown.Features.User.UserRegister;
using GoldenCrown.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GoldenCrown.Tests;

public sealed class CqrsTests : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IServiceScope scope;
    private readonly ApplicationDbContext db;
    private readonly ISender sender;

    public CqrsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(UserRegisterCommand).Assembly));
        provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        scope = provider.CreateScope();
        db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        sender = scope.ServiceProvider.GetRequiredService<ISender>();
    }

    private async Task<User> Register(string login)
    {
        Assert.True((await sender.Send(new UserRegisterCommand(login, login, "password123"))).IsSuccess);
        return await db.Users.Include(user => user.Account).SingleAsync(user => user.Login == login);
    }

    [Fact]
    public async Task RegistrationCreatesUserWithZeroBalanceAndHashedPassword()
    {
        var user = await Register("alice");
        Assert.Equal(0m, user.Account.Balance);
        Assert.Equal(user.Id, user.Account.UserId);
        Assert.NotEqual("password123", user.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, "password123"));
    }

    [Fact]
    public async Task DuplicateRegistrationDoesNotCreateAnotherUserOrAccount()
    {
        await Register("alice");
        var result = await sender.Send(new UserRegisterCommand("alice", "Other", "password456"));
        Assert.False(result.IsSuccess);
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.Accounts.CountAsync());
    }

    [Fact]
    public async Task LoginRotatesTokenAndKeepsOneSession()
    {
        var user = await Register("alice");
        var first = await sender.Send(new UserLoginCommand("alice", "password123"));
        var second = await sender.Send(new UserLoginCommand("alice", "password123"));
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.NotEqual(first.Value, second.Value);
        var session = Assert.Single(await db.Sessions.ToListAsync());
        Assert.Equal(user.Id, session.UserId);
        Assert.Equal(second.Value, session.Token);
        Assert.True(session.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task IncorrectPasswordDoesNotChangeExistingSession()
    {
        await Register("alice");
        var valid = await sender.Send(new UserLoginCommand("alice", "password123"));
        var invalid = await sender.Send(new UserLoginCommand("alice", "wrong"));
        Assert.False(invalid.IsSuccess);
        Assert.Equal(valid.Value, (await db.Sessions.SingleAsync()).Token);
    }

    [Fact]
    public async Task LegacyPasswordIsUpgradedOnLogin()
    {
        var user = new User { Login = "legacy", Name = "Legacy", PasswordHash = "old-password" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        Assert.True((await sender.Send(new UserLoginCommand("legacy", "old-password"))).IsSuccess);
        Assert.NotEqual("old-password", user.PasswordHash);
    }

    [Fact]
    public async Task CreateAccountHandlesExistingUserAndRejectsDuplicate()
    {
        var user = new User { Login = "alice", Name = "Alice" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await sender.Send(new CreateAccountCommand("alice"));
        Assert.Equal(0m, (await db.Accounts.SingleAsync()).Balance);
        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.Send(new CreateAccountCommand("alice")));
        Assert.Equal(1, await db.Accounts.CountAsync());
    }

    [Fact]
    public async Task CreateAccountRejectsMissingUser()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.Send(new CreateAccountCommand("missing")));
        Assert.Empty(await db.Accounts.ToListAsync());
    }

    [Fact]
    public async Task DepositPersistsBalance()
    {
        var user = await Register("alice");
        Assert.True((await sender.Send(new DepositCommand(user.Id, 100m))).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Equal(100m, (await db.Accounts.SingleAsync()).Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task NonPositiveDepositDoesNotChangeBalance(decimal amount)
    {
        var user = await Register("alice");
        Assert.False((await sender.Send(new DepositCommand(user.Id, amount))).IsSuccess);
        Assert.Equal(0m, (await db.Accounts.SingleAsync()).Balance);
    }

    [Fact]
    public async Task TransferUpdatesBothBalancesAndRecordsTransaction()
    {
        var alice = await Register("alice");
        var bob = await Register("bob");
        await sender.Send(new DepositCommand(alice.Id, 100m));
        Assert.True((await sender.Send(new TransferCommand(alice.Id, "bob", 30m))).IsSuccess);
        db.ChangeTracker.Clear();
        Assert.Equal(70m, (await db.Accounts.SingleAsync(a => a.UserId == alice.Id)).Balance);
        Assert.Equal(30m, (await db.Accounts.SingleAsync(a => a.UserId == bob.Id)).Balance);
        var transaction = await db.Transactions.SingleAsync();
        Assert.Equal(alice.Id, transaction.SenderId);
        Assert.Equal(bob.Id, transaction.ReceiverId);
        Assert.Equal(30m, transaction.Amount);
    }

    [Theory]
    [InlineData("alice", 10)]
    [InlineData("bob", 101)]
    [InlineData("missing", 10)]
    [InlineData("bob", 0)]
    public async Task RejectedTransferDoesNotChangeBalancesOrHistory(string receiver, decimal amount)
    {
        var alice = await Register("alice");
        await Register("bob");
        await sender.Send(new DepositCommand(alice.Id, 100m));
        Assert.False((await sender.Send(new TransferCommand(alice.Id, receiver, amount))).IsSuccess);
        Assert.Equal(100m, alice.Account.Balance);
        Assert.Equal(0m, (await db.Accounts.SingleAsync(a => a.UserId != alice.Id)).Balance);
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    [Fact]
    public async Task BalanceQueryDoesNotTrackEntities()
    {
        var user = await Register("alice");
        await sender.Send(new DepositCommand(user.Id, 12m));
        db.ChangeTracker.Clear();
        var balance = await sender.Send(new GetBalanceQuery(user.Id));
        Assert.True(balance.IsSuccess);
        Assert.Equal(12m, balance.Value);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.False((await sender.Send(new GetBalanceQuery(-1))).IsSuccess);
    }

    [Fact]
    public async Task HistoryFiltersUserDatesAndPaginationWithoutTracking()
    {
        var alice = await Register("alice");
        var bob = await Register("bob");
        var carol = await Register("carol");
        var date = DateTimeOffset.UtcNow;
        db.Transactions.AddRange(
            new Transaction { SenderId = alice.Id, ReceiverId = bob.Id, Amount = 1, Date = date.AddDays(-2) },
            new Transaction { SenderId = bob.Id, ReceiverId = alice.Id, Amount = 2, Date = date },
            new Transaction { SenderId = alice.Id, ReceiverId = bob.Id, Amount = 3, Date = date },
            new Transaction { SenderId = bob.Id, ReceiverId = carol.Id, Amount = 4, Date = date });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var history = await sender.Send(new GetTransactionHistoryQuery(alice.Id, date, date, 1, 1));
        Assert.True(history.IsSuccess);
        var item = Assert.Single(history.Value);
        Assert.Equal(2m, item.Amount);
        Assert.Equal("bob", item.SenderName);
        Assert.Equal("alice", item.ReceiverName);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task HistoryRejectsInvalidBounds()
    {
        var date = DateTimeOffset.UtcNow;
        Assert.False((await sender.Send(new GetTransactionHistoryQuery(1, date, date.AddDays(-1), 0, 10))).IsSuccess);
        Assert.False((await sender.Send(new GetTransactionHistoryQuery(1, null, null, -1, 10))).IsSuccess);
        Assert.False((await sender.Send(new GetTransactionHistoryQuery(1, null, null, 0, 0))).IsSuccess);
    }

    [Fact]
    public async Task CleanupDeletesOnlyExpiredSessions()
    {
        var alice = await Register("alice");
        var bob = await Register("bob");
        db.Sessions.AddRange(
            new Session { UserId = alice.Id, Token = "expired", ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1) },
            new Session { UserId = bob.Id, Token = "active", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) });
        await db.SaveChangesAsync();
        Assert.Equal(1, await sender.Send(new CleanupExpiredSessionsCommand()));
        Assert.Equal("active", (await db.Sessions.SingleAsync()).Token);
    }

    [Fact]
    public async Task CancelledCommandDoesNotCreateUser()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sender.Send(new UserRegisterCommand("alice", "Alice", "password123"), cancellation.Token));
        Assert.Empty(await db.Users.ToListAsync());
    }

    public void Dispose()
    {
        scope.Dispose();
        provider.Dispose();
    }
}
