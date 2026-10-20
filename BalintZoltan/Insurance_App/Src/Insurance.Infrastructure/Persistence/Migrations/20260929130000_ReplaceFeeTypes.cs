using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InsuranceDbContext))]
[Migration("20260929130000_ReplaceFeeTypes")]
public partial class ReplaceFeeTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE FeeConfigurations SET Type = 'AdminFee' WHERE Type IN ('Percentage', 'FixedAmount');");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE FeeConfigurations SET Type = 'Percentage' WHERE Type IN ('BrokerCommission', 'RiskAdjustment', 'AdminFee');");
    }
}
