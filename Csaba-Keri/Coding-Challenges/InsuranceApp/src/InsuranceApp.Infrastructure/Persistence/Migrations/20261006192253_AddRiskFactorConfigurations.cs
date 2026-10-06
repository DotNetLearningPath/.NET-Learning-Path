using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskFactorConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "risk_factor_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    county_id = table.Column<Guid>(type: "uuid", nullable: true),
                    city_id = table.Column<Guid>(type: "uuid", nullable: true),
                    building_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    adjustment_percentage = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_factor_configurations", x => x.id);
                    table.CheckConstraint("ck_risk_factor_configurations_adjustment_percentage", "adjustment_percentage BETWEEN -100 AND 100");
                    table.CheckConstraint("ck_risk_factor_configurations_building_type", "building_type IS NULL OR building_type IN ('Residential', 'Office', 'Industrial')");
                    table.CheckConstraint("ck_risk_factor_configurations_target", "(level = 'Country' AND country_id IS NOT NULL AND county_id IS NULL AND city_id IS NULL AND building_type IS NULL)\r\nOR (level = 'County' AND county_id IS NOT NULL AND country_id IS NULL AND city_id IS NULL AND building_type IS NULL)\r\nOR (level = 'City' AND city_id IS NOT NULL AND country_id IS NULL AND county_id IS NULL AND building_type IS NULL)\r\nOR (level = 'BuildingType' AND building_type IS NOT NULL AND country_id IS NULL AND county_id IS NULL AND city_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_risk_factor_configurations_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_risk_factor_configurations_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_risk_factor_configurations_county_id",
                        column: x => x.county_id,
                        principalTable: "counties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_risk_factor_configurations_city_id",
                table: "risk_factor_configurations",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_factor_configurations_country_id",
                table: "risk_factor_configurations",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_factor_configurations_county_id",
                table: "risk_factor_configurations",
                column: "county_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_factor_configurations_is_active_building_type",
                table: "risk_factor_configurations",
                columns: new[] { "is_active", "building_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "risk_factor_configurations");
        }
    }
}
