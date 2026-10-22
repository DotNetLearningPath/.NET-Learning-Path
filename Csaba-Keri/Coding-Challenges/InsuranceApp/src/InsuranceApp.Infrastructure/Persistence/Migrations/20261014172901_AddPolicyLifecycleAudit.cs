using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyLifecycleAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "activated_at",
                table: "policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cancellation_effective_date",
                table: "policies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "policies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expired_at",
                table: "policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_policies_status_end_date",
                table: "policies",
                columns: new[] { "status", "end_date" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_policies_cancellation_audit",
                table: "policies",
                sql: "(cancelled_at IS NULL AND cancellation_effective_date IS NULL AND cancellation_reason IS NULL)\r\nOR (status = 'Cancelled' AND cancelled_at IS NOT NULL AND cancellation_effective_date IS NOT NULL\r\nAND cancellation_reason IS NOT NULL AND length(btrim(cancellation_reason)) > 0\r\nAND cancellation_effective_date = (cancelled_at AT TIME ZONE 'UTC')::date\r\nAND cancellation_effective_date <= end_date)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_policies_expiry_audit",
                table: "policies",
                sql: "expired_at IS NULL OR (status = 'Expired' AND end_date < (expired_at AT TIME ZONE 'UTC')::date)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_policies_lifecycle_timestamps",
                table: "policies",
                sql: "updated_at >= created_at\r\nAND (activated_at IS NULL OR (activated_at >= created_at AND activated_at <= updated_at AND status <> 'Draft'))\r\nAND (cancelled_at IS NULL OR (cancelled_at >= created_at AND cancelled_at <= updated_at\r\nAND (activated_at IS NULL OR cancelled_at >= activated_at)))\r\nAND (expired_at IS NULL OR (expired_at >= created_at AND expired_at <= updated_at\r\nAND (activated_at IS NULL OR expired_at >= activated_at)))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_policies_status_end_date",
                table: "policies");

            migrationBuilder.DropCheckConstraint(
                name: "ck_policies_cancellation_audit",
                table: "policies");

            migrationBuilder.DropCheckConstraint(
                name: "ck_policies_expiry_audit",
                table: "policies");

            migrationBuilder.DropCheckConstraint(
                name: "ck_policies_lifecycle_timestamps",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "activated_at",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "cancellation_effective_date",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "expired_at",
                table: "policies");
        }
    }
}
