using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskFactorConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskFactorConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuildingType = table.Column<int>(type: "int", nullable: true),
                    AdjustmentPercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorConfigurations", x => x.Id);
                    table.CheckConstraint("CK_RiskFactorConfigurations_LevelTarget", "([Level] = 1 AND [CountryId] IS NOT NULL AND [CountyId] IS NULL AND [CityId] IS NULL AND [BuildingType] IS NULL) OR ([Level] = 2 AND [CountryId] IS NULL AND [CountyId] IS NOT NULL AND [CityId] IS NULL AND [BuildingType] IS NULL) OR ([Level] = 3 AND [CountryId] IS NULL AND [CountyId] IS NULL AND [CityId] IS NOT NULL AND [BuildingType] IS NULL) OR ([Level] = 4 AND [CountryId] IS NULL AND [CountyId] IS NULL AND [CityId] IS NULL AND [BuildingType] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RiskFactorConfigurations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskFactorConfigurations_Counties_CountyId",
                        column: x => x.CountyId,
                        principalTable: "Counties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskFactorConfigurations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RiskFactorConfigurations",
                columns: new[] { "Id", "AdjustmentPercentage", "BuildingType", "CityId", "CountryId", "CountyId", "IsActive", "Level" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1"), 5m, 1, null, null, null, true, 4 },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2"), 10m, 3, null, null, null, true, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_CityId",
                table: "RiskFactorConfigurations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_CountryId",
                table: "RiskFactorConfigurations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_CountyId",
                table: "RiskFactorConfigurations",
                column: "CountyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskFactorConfigurations");
        }
    }
}
