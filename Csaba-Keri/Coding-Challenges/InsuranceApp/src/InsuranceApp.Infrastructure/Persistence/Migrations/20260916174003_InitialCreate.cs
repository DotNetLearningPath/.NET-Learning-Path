using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    identification_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "C"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    primary_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.CheckConstraint("ck_clients_type", "type IN ('Individual', 'Company')");
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "counties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counties", x => x.id);
                    table.ForeignKey(
                        name: "FK_counties_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    county_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.id);
                    table.ForeignKey(
                        name: "FK_cities_counties_county_id",
                        column: x => x.county_id,
                        principalTable: "counties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    city_id = table.Column<Guid>(type: "uuid", nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    construction_year = table.Column<int>(type: "integer", nullable: false),
                    number_of_floors = table.Column<int>(type: "integer", nullable: false),
                    surface_area = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    insured_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buildings", x => x.id);
                    table.CheckConstraint("ck_buildings_construction_year", "construction_year BETWEEN 1 AND 9999");
                    table.CheckConstraint("ck_buildings_insured_value", "insured_value > 0");
                    table.CheckConstraint("ck_buildings_number_of_floors", "number_of_floors >= 1");
                    table.CheckConstraint("ck_buildings_surface_area", "surface_area > 0");
                    table.CheckConstraint("ck_buildings_type", "type IN ('Residential', 'Office', 'Industrial')");
                    table.ForeignKey(
                        name: "fk_buildings_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_buildings_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "Romania" },
                    { new Guid("11111111-1111-4111-8111-111111111112"), "Hungary" }
                });

            migrationBuilder.InsertData(
                table: "counties",
                columns: new[] { "id", "country_id", "name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("11111111-1111-4111-8111-111111111111"), "Cluj" },
                    { new Guid("22222222-2222-4222-8222-222222222223"), new Guid("11111111-1111-4111-8111-111111111112"), "Hajdú-Bihar" }
                });

            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "id", "county_id", "name" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-4333-8333-333333333333"), new Guid("22222222-2222-4222-8222-222222222222"), "Cluj-Napoca" },
                    { new Guid("33333333-3333-4333-8333-333333333334"), new Guid("22222222-2222-4222-8222-222222222222"), "Turda" },
                    { new Guid("33333333-3333-4333-8333-333333333335"), new Guid("22222222-2222-4222-8222-222222222223"), "Debrecen" },
                    { new Guid("33333333-3333-4333-8333-333333333336"), new Guid("22222222-2222-4222-8222-222222222223"), "Hajdúszoboszló" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_buildings_city_id",
                table: "buildings",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_buildings_client_id_id",
                table: "buildings",
                columns: new[] { "client_id", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_cities_county_id_name",
                table: "cities",
                columns: new[] { "county_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_name_id",
                table: "clients",
                columns: new[] { "name", "id" });

            migrationBuilder.CreateIndex(
                name: "ux_clients_identification_number",
                table: "clients",
                column: "identification_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_counties_country_id_name",
                table: "counties",
                columns: new[] { "country_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_countries_name",
                table: "countries",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "buildings");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "counties");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
