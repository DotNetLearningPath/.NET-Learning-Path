using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRiskFactorReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "RiskFactorConfigurations_New" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_RiskFactorConfigurations_New" PRIMARY KEY,
                    "Level" TEXT NOT NULL,
                    "Reference" TEXT NOT NULL,
                    "AdjustmentPercentage" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL
                );

                INSERT INTO "RiskFactorConfigurations_New"
                    ("Id", "Level", "Reference", "AdjustmentPercentage", "IsActive")
                SELECT "Id", "Level",
                    CASE WHEN "BuildingTypeReference" IS NOT NULL
                         THEN "BuildingTypeReference"
                         ELSE "ReferenceId"
                    END,
                    "AdjustmentPercentage", "IsActive"
                FROM "RiskFactorConfigurations";

                DROP TABLE "RiskFactorConfigurations";
                ALTER TABLE "RiskFactorConfigurations_New"
                    RENAME TO "RiskFactorConfigurations";

                CREATE UNIQUE INDEX "IX_RiskFactorConfigurations_Level_Reference"
                    ON "RiskFactorConfigurations" ("Level", "Reference");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "RiskFactorConfigurations_Old" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_RiskFactorConfigurations_Old" PRIMARY KEY,
                    "Level" TEXT NOT NULL,
                    "ReferenceId" TEXT NULL,
                    "BuildingTypeReference" TEXT NULL,
                    "AdjustmentPercentage" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL
                );

                INSERT INTO "RiskFactorConfigurations_Old"
                    ("Id", "Level", "ReferenceId", "BuildingTypeReference",
                     "AdjustmentPercentage", "IsActive")
                SELECT "Id", "Level",
                    CASE WHEN "Level" = 'BuildingType' THEN NULL ELSE "Reference" END,
                    CASE WHEN "Level" = 'BuildingType' THEN "Reference" ELSE NULL END,
                    "AdjustmentPercentage", "IsActive"
                FROM "RiskFactorConfigurations";

                DROP TABLE "RiskFactorConfigurations";
                ALTER TABLE "RiskFactorConfigurations_Old"
                    RENAME TO "RiskFactorConfigurations";

                CREATE UNIQUE INDEX "IX_RiskFactorConfigurations_Level_ReferenceId_BuildingTypeReference"
                    ON "RiskFactorConfigurations"
                    ("Level", "ReferenceId", "BuildingTypeReference");
                """);
        }
    }
}
