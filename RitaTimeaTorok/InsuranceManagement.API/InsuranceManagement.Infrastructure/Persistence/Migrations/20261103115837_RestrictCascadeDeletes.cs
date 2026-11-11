using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestrictCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Clients_ClientId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Clients_ClientId",
                table: "Buildings",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Clients_ClientId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Clients_ClientId",
                table: "Buildings",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
