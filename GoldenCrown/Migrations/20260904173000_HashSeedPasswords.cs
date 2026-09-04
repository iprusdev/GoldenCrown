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
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJNvqd+NoFqelrNYynzWs+Eqa2zJ4J6GN+pAKRvspk38g4V3UTB1jHNdgO/EZUsP2Q==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEInhvCnFwcUP+83KkhVErZskd9Wvyo/j3bnUILGdt7Ta2x3NPqD2wpACyH8IZ4E2SA==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGYzKMpM9Dtvit54wAx0NYpKHdvlqXggThin4CelAx2OWQxgpEPzNzfr7nQ+vbxuMA==");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "seed-test-hash-1");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "seed-test-hash-2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "seed-test-hash-3");
        }
    }
}
