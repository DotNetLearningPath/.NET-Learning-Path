using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskFactorBuildingTypeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "RiskFactorConfigurations_New" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_RiskFactorConfigurations_New" PRIMARY KEY,
                    "Level" TEXT NOT NULL,
                    "ReferenceId" TEXT NULL,
                    "BuildingTypeReference" TEXT NULL,
                    "AdjustmentPercentage" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL
                );

                INSERT INTO "RiskFactorConfigurations_New"
                    ("Id", "Level", "ReferenceId", "AdjustmentPercentage", "IsActive")
                SELECT "Id", "Level", "ReferenceId", "AdjustmentPercentage", "IsActive"
                FROM "RiskFactorConfigurations";

                DROP TABLE "RiskFactorConfigurations";
                ALTER TABLE "RiskFactorConfigurations_New"
                    RENAME TO "RiskFactorConfigurations";

                CREATE UNIQUE INDEX "IX_RiskFactorConfigurations_Level_ReferenceId_BuildingTypeReference"
                    ON "RiskFactorConfigurations"
                    ("Level", "ReferenceId", "BuildingTypeReference");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "RiskFactorConfigurations_Old" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_RiskFactorConfigurations_Old" PRIMARY KEY,
                    "Level" TEXT NOT NULL,
                    "ReferenceId" TEXT NOT NULL,
                    "AdjustmentPercentage" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL
                );

                INSERT INTO "RiskFactorConfigurations_Old"
                    ("Id", "Level", "ReferenceId", "AdjustmentPercentage", "IsActive")
                SELECT "Id", "Level", "ReferenceId", "AdjustmentPercentage", "IsActive"
                FROM "RiskFactorConfigurations"
                WHERE "ReferenceId" IS NOT NULL;

                DROP TABLE "RiskFactorConfigurations";
                ALTER TABLE "RiskFactorConfigurations_Old"
                    RENAME TO "RiskFactorConfigurations";

                CREATE UNIQUE INDEX "IX_RiskFactorConfigurations_Level_ReferenceId"
                    ON "RiskFactorConfigurations" ("Level", "ReferenceId");
                """);
        }
    }
}
