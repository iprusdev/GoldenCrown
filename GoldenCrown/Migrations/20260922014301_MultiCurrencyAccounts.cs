using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldenCrown.Migrations
{
    /// <inheritdoc />
    public partial class MultiCurrencyAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Transactions",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "BYN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Accounts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "BYN");

            // Existing balances and history belong to BYN; create only missing accounts.
            migrationBuilder.Sql("""
                INSERT INTO [Accounts] ([UserId], [Balance], [Currency])
                SELECT u.[Id], 0, c.[Currency]
                FROM [Users] u
                CROSS JOIN (VALUES ('USD'), ('EUR'), ('BYN')) c([Currency])
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Accounts] a
                    WHERE a.[UserId] = u.[Id] AND a.[Currency] = c.[Currency]);
                """);
            migrationBuilder.AddCheckConstraint(
                name: "CK_Transactions_Currency",
                table: "Transactions",
                sql: "[Currency] IN ('USD', 'EUR', 'BYN')");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_Currency",
                table: "Accounts",
                columns: new[] { "UserId", "Currency" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accounts_Currency",
                table: "Accounts",
                sql: "[Currency] IN ('USD', 'EUR', 'BYN')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Never discard balances or reinterpret foreign-currency history during rollback.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Accounts] WHERE [Currency] <> 'BYN' AND [Balance] <> 0)
                   OR EXISTS (SELECT 1 FROM [Transactions] WHERE [Currency] <> 'BYN')
                    THROW 50001, 'Cannot roll back while USD/EUR balances or transactions exist.', 1;
                DELETE FROM [Accounts] WHERE [Currency] <> 'BYN';
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transactions_Currency",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_Currency",
                table: "Accounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accounts_Currency",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId",
                unique: true);
        }
    }
}
