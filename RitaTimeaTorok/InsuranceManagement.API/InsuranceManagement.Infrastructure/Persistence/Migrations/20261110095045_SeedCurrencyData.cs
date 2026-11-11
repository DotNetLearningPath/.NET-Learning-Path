using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCurrencyData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "ExchangeRateToBase", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-4666-8666-666666666666"), "RON", 1.00m, true, "Romanian Leu" },
                    { new Guid("77777777-7777-4777-8777-777777777777"), "EUR", 5.00m, true, "Euro" },
                    { new Guid("88888888-8888-4888-8888-888888888888"), "USD", 4.50m, true, "US Dollar" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-4666-8666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-4777-8777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-4888-8888-888888888888"));
        }
    }
}
