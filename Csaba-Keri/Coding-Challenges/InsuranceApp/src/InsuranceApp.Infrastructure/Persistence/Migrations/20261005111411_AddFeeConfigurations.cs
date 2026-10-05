using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fee_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_configurations", x => x.id);
                    table.CheckConstraint("ck_fee_configurations_effective_period", "effective_to IS NULL OR effective_to >= effective_from");
                    table.CheckConstraint("ck_fee_configurations_percentage", "percentage BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_fee_configurations_type", "type IN ('BrokerCommission', 'RiskAdjustment', 'AdminFee')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_fee_configurations_is_active_effective_from",
                table: "fee_configurations",
                columns: new[] { "is_active", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "IX_fee_configurations_name_id",
                table: "fee_configurations",
                columns: new[] { "name", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fee_configurations");
        }
    }
}
