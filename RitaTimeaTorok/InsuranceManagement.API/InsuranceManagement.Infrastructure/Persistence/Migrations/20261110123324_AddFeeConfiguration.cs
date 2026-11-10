using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeeConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeConfigurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FeeConfigurations",
                columns: new[] { "Id", "EffectiveFrom", "EffectiveTo", "IsActive", "Name", "Percentage", "Type" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-4999-8999-999999999991"), new DateOnly(2026, 1, 1), null, true, "Standard broker fee", 5m, "BrokerCommission" },
                    { new Guid("99999999-9999-4999-8999-999999999992"), new DateOnly(2026, 1, 1), null, true, "Standard admin fee", 2m, "AdminFee" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeConfigurations");
        }
    }
}
