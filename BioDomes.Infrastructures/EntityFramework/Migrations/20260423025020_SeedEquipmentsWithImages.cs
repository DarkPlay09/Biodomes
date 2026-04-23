using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SeedEquipmentsWithImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Equipments",
                columns: new[] { "Id", "ConsumedElement", "CreatorId", "ImagePath", "IsPublicAvailable", "Name", "ProducedElement" },
                values: new object[,]
                {
                    { 1, "Hydrogene", 1, "/images/equipment/helio-lamp-a7f3k2q1.jpg", true, "Helio Lamp A7F3K2Q1", "Lumiere" },
                    { 2, "Azote", 1, "/images/equipment/uv-array-b9m4d8r2.jpg", true, "UV Array B9M4D8R2", "Lumiere" },
                    { 3, "Hydrogene", 1, "/images/equipment/micro-pump-c6p1t7l5.jpg", true, "Micro Pump C6P1T7L5", "Eau" },
                    { 4, "Eau", 1, "/images/equipment/nitro-filter-d3x8n4v6.jpg", true, "Nitro Filter D3X8N4V6", "Azote" },
                    { 5, "Eau", 1, "/images/equipment/hydro-cell-e2r9j5s8.jpg", true, "Hydro Cell E2R9J5S8", "Hydrogene" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
