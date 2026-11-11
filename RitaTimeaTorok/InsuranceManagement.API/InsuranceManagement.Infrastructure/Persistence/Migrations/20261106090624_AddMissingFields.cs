using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Buildings",
                newName: "Street");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryAddress",
                table: "Clients",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StreetNumber",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryAddress",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "StreetNumber",
                table: "Buildings");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Buildings",
                newName: "Address");
        }
    }
}
