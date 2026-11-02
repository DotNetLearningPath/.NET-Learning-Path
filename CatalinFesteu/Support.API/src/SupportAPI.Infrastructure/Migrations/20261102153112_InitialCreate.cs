using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "support-db");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "support-db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IdentityIssuer = table.Column<string>(type: "text", nullable: false),
                    IdentitySubject = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                schema: "support-db",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestContent = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "support-db",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_UserId",
                schema: "support-db",
                table: "Requests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_Users_IdentityIssuer_IdentitySubject",
                schema: "support-db",
                table: "Users",
                columns: new[] { "IdentityIssuer", "IdentitySubject" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests",
                schema: "support-db");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "support-db");
        }
    }
}
