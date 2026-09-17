using GoldenCrown.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldenCrown.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260904173000_HashSeedPasswords")]
    public partial class HashSeedPasswords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 1 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "AQAAAAIAAYagAAAAEJNvqd+NoFqelrNYynzWs+Eqa2zJ4J6GN+pAKRvspk38g4V3UTB1jHNdgO/EZUsP2Q==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 2 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "AQAAAAIAAYagAAAAEInhvCnFwcUP+83KkhVErZskd9Wvyo/j3bnUILGdt7Ta2x3NPqD2wpACyH8IZ4E2SA==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 3 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "AQAAAAIAAYagAAAAEGYzKMpM9Dtvit54wAx0NYpKHdvlqXggThin4CelAx2OWQxgpEPzNzfr7nQ+vbxuMA==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 1 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "seed-test-hash-1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 2 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "seed-test-hash-2" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Id" },
                keyColumnTypes: new[] { "int" },
                keyValues: new object[] { 3 },
                columns: new[] { "PasswordHash" },
                columnTypes: new[] { "nvarchar(500)" },
                values: new object[] { "seed-test-hash-3" });
        }
    }
}
