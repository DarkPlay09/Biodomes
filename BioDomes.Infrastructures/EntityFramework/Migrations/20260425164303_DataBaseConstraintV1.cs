using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class DataBaseConstraintV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_BirthDate_NotFuture",
                table: "Users",
                sql: "[BirthDate] <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Role_AllowedValues",
                table: "Users",
                sql: "[Role] IN ('User', 'Admin')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Species_AdultSizeAndWeight_Positive",
                table: "Species",
                sql: "[AdultSize] > 0 AND  [Weight] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Species_Name_NotBlank",
                table: "Species",
                sql: "[Name] <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipment_Name_NotBlank",
                table: "Equipments",
                sql: "[Name] <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipment_ProducedOrConsumed_Required",
                table: "Equipments",
                sql: "[ProducedElement] IS NOT NULL OR [ConsumedElement] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BiomeSpecies_IndividualCount_Positive",
                table: "BiomeSpecies",
                sql: "[IndividualCount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Biomes_AbsoluteHumidity_NonNegative",
                table: "Biomes",
                sql: "[AbsoluteHumidity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Biomes_State_Enum",
                table: "Biomes",
                sql: "[State] IN ('Optimal','Instable','Critique')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes",
                sql: "[Temperature] >= -100 AND [Temperature] <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_BirthDate_NotFuture",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Role_AllowedValues",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Species_AdultSizeAndWeight_Positive",
                table: "Species");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Species_Name_NotBlank",
                table: "Species");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipment_Name_NotBlank",
                table: "Equipments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipment_ProducedOrConsumed_Required",
                table: "Equipments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BiomeSpecies_IndividualCount_Positive",
                table: "BiomeSpecies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Biomes_AbsoluteHumidity_NonNegative",
                table: "Biomes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Biomes_State_Enum",
                table: "Biomes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes");
        }
    }
}
