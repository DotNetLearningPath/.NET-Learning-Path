using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameFeeValueToPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "FeeConfigurations",
                newName: "Percentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "FeeConfigurations",
                newName: "Value");
        }
    }
}
