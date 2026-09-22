using GoldenCrown.Data;
using GoldenCrown.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace GoldenCrown.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("GOLDENCROWN_SQL_TESTS") != "1")
            Skip = "Set GOLDENCROWN_SQL_TESTS=1 to test migrations on local SQL Server Express.";
    }
}

public sealed class CurrencyMigrationTests
{
    [SqlServerFact]
    public async Task MigrationPreservesLegacyMoneyCreatesMissingAccountsAndEnforcesConstraints()
    {
        var databaseName = "GoldenCrown_CurrencyTest_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer($@"Server=.\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        await using var db = new ApplicationDbContext(options);
        try
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20260904173000_HashSeedPasswords");
            // The old schema has one account per user and no currency columns.
            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM [Accounts] WHERE [UserId] = 1)
                    INSERT INTO [Accounts] ([UserId], [Balance]) VALUES (1, 123.45);
                ELSE UPDATE [Accounts] SET [Balance] = 123.45 WHERE [UserId] = 1;
                INSERT INTO [Transactions] ([SenderId], [ReceiverId], [Amount], [Date])
                VALUES (1, 2, 12.34, SYSDATETIMEOFFSET());
                """);
            await migrator.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
            var accounts = await db.Accounts.AsNoTracking().ToListAsync();
            var userCount = await db.Users.CountAsync();
            Assert.Equal(userCount * 3, accounts.Count);
            Assert.Equal(123.45m, accounts.Single(a => a.UserId == 1 && a.Currency == Currency.BYN).Balance);
            Assert.All(accounts.Where(a => a.Currency != Currency.BYN), a => Assert.Equal(0m, a.Balance));
            Assert.All(await db.Transactions.ToListAsync(), t => Assert.Equal(Currency.BYN, t.Currency));
            Assert.Contains(await db.Transactions.ToListAsync(), t => t.Amount == 12.34m);
            await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
                "INSERT INTO [Accounts] ([UserId], [Balance], [Currency]) VALUES (1, 0, 'USD')"));
            await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
                "INSERT INTO [Accounts] ([UserId], [Balance], [Currency]) VALUES (1, 0, 'RUB')"));
            await db.Database.ExecuteSqlRawAsync("UPDATE [Accounts] SET [Balance] = 1 WHERE [UserId] = 1 AND [Currency] = 'USD'");
            await Assert.ThrowsAsync<SqlException>(() => migrator.MigrateAsync("20260904173000_HashSeedPasswords"));
            await db.Database.ExecuteSqlRawAsync("UPDATE [Accounts] SET [Balance] = 0 WHERE [UserId] = 1 AND [Currency] = 'USD'");
            await migrator.MigrateAsync("20260904173000_HashSeedPasswords");
            await migrator.MigrateAsync();
            Assert.Equal(123.45m, await db.Accounts.Where(a => a.UserId == 1 && a.Currency == Currency.BYN).Select(a => a.Balance).SingleAsync());
            Assert.Equal(userCount * 3, await db.Accounts.CountAsync());
        }
        finally
        {
            // Only the GUID-named database created by this test is removed.
            await db.Database.EnsureDeletedAsync();
        }
    }
}
