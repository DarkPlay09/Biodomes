using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class LocalSpeciesImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "/images/species/lion-dafrique-a1b2c3d4.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "/images/species/monstera-b2c3d4e5.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "/images/species/ara-rouge-c3d4e5f6.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "/images/species/tortue-geante-d4e5f6a7.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "/images/species/lemur-catta-e5f6a7b8.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "/images/species/sequoia-geant-f6a7b8c9.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "/images/species/aloe-vera-a7b8c9d0.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "/images/species/loup-gris-b8c9d0e1.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "/images/species/raton-laveur-c9d0e1f2.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "/images/species/boa-constrictor-d0e1f2a3.jpg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/3/3e/Lion_d%27Afrique_%28Panthera_leo%29.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/e/ef/Monstera_deliciosa_Leaf_2700px.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/5/5f/Scarlet_macaw_ara_macao.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/1/16/Giant_Tortoise.JPG");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/9/90/Katta_Lemur_catta.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/1/18/The_giant_sequoia_trees.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/0/07/Aloe_vera_plant_in_flower_pot.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/e/e8/Loup_gris_%28Canis_lupus_%29.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/f/fb/Raccoon_%28Procyon_lotor%29%2C_portrait.jpg");

            migrationBuilder.UpdateData(
                table: "Species",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/e/ed/Boa_constrictor%2C_boa_constrictora.jpg");
        }
    }
}
