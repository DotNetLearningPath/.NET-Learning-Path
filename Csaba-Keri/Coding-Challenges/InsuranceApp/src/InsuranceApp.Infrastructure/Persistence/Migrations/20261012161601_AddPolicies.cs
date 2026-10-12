using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false),
                    broker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency_exchange_rate_to_base = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    base_premium = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    final_premium = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.id);
                    table.CheckConstraint("ck_policies_currency_exchange_rate_to_base", "currency_exchange_rate_to_base > 0");
                    table.CheckConstraint("ck_policies_period", "end_date >= start_date");
                    table.CheckConstraint("ck_policies_premiums", "base_premium > 0 AND final_premium > 0");
                    table.CheckConstraint("ck_policies_status", "status IN ('Draft', 'Active', 'Expired', 'Cancelled')");
                    table.ForeignKey(
                        name: "fk_policies_broker_id",
                        column: x => x.broker_id,
                        principalTable: "brokers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policies_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policies_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_policies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "policy_adjustments",
                columns: table => new
                {
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_adjustments", x => new { x.policy_id, x.source, x.configuration_id });
                    table.CheckConstraint("ck_policy_adjustments_percentage", "(source = 'Fee' AND percentage BETWEEN 0 AND 100)\r\nOR (source = 'RiskFactor' AND percentage BETWEEN -100 AND 100)");
                    table.CheckConstraint("ck_policy_adjustments_source", "source IN ('Fee', 'RiskFactor')");
                    table.ForeignKey(
                        name: "FK_policy_adjustments_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policies_broker_id",
                table: "policies",
                column: "broker_id");

            migrationBuilder.CreateIndex(
                name: "IX_policies_building_id",
                table: "policies",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_policies_client_id",
                table: "policies",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_policies_currency_id",
                table: "policies",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_policies_policy_number",
                table: "policies",
                column: "policy_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_adjustments");

            migrationBuilder.DropTable(
                name: "policies");
        }
    }
}
