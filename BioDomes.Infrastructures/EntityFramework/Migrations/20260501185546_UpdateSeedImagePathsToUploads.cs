using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedImagePathsToUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePath",
                value: "/uploads/equipment/helio-lamp-a7f3k2q1.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePath",
                value: "/uploads/equipment/uv-array-b9m4d8r2.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagePath",
                value: "/uploads/equipment/micro-pump-c6p1t7l5.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagePath",
                value: "/uploads/equipment/nitro-filter-d3x8n4v6.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagePath",
                value: "/uploads/equipment/hydro-cell-e2r9j5s8.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePath",
                value: "/uploads/species/lion-dafrique-a1b2c3d4.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePath",
                value: "/uploads/species/monstera-b2c3d4e5.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagePath",
                value: "/uploads/species/ara-rouge-c3d4e5f6.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagePath",
                value: "/uploads/species/tortue-geante-d4e5f6a7.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagePath",
                value: "/uploads/species/lemur-catta-e5f6a7b8.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImagePath",
                value: "/uploads/species/sequoia-geant-f6a7b8c9.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImagePath",
                value: "/uploads/species/aloe-vera-a7b8c9d0.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImagePath",
                value: "/uploads/species/loup-gris-b8c9d0e1.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImagePath",
                value: "/uploads/species/raton-laveur-c9d0e1f2.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImagePath",
                value: "/uploads/species/boa-constrictor-d0e1f2a3.jpg");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes",
                sql: "[Temperature] >= -100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePath",
                value: "/images/equipment/helio-lamp-a7f3k2q1.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePath",
                value: "/images/equipment/uv-array-b9m4d8r2.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagePath",
                value: "/images/equipment/micro-pump-c6p1t7l5.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagePath",
                value: "/images/equipment/nitro-filter-d3x8n4v6.jpg");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagePath",
                value: "/images/equipment/hydro-cell-e2r9j5s8.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePath",
                value: "/images/species/lion-dafrique-a1b2c3d4.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePath",
                value: "/images/species/monstera-b2c3d4e5.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagePath",
                value: "/images/species/ara-rouge-c3d4e5f6.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagePath",
                value: "/images/species/tortue-geante-d4e5f6a7.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagePath",
                value: "/images/species/lemur-catta-e5f6a7b8.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImagePath",
                value: "/images/species/sequoia-geant-f6a7b8c9.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImagePath",
                value: "/images/species/aloe-vera-a7b8c9d0.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImagePath",
                value: "/images/species/loup-gris-b8c9d0e1.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImagePath",
                value: "/images/species/raton-laveur-c9d0e1f2.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImagePath",
                value: "/images/species/boa-constrictor-d0e1f2a3.jpg");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Biomes_Temperature_Range",
                table: "Biomes",
                sql: "[Temperature] >= -100 AND [Temperature] <= 100");
        }
    }
}
