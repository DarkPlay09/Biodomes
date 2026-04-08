using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Diet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdultSize = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "AdultSize", "Classification", "CreatedByUserName", "Diet", "ImagePath", "IsPublic", "Name", "Weight" },
                values: new object[,]
                {
                    { 1, 2.5, "Mammifère", "admin", "Carnivore", "/images/species/lion-dafrique-a1b2c3d4.jpg", true, "Lion d'Afrique", 190.0 },
                    { 2, 9.0, "Plante", "admin", "Photosynthèse", "/images/species/monstera-b2c3d4e5.jpg", true, "Monstera", 12.0 },
                    { 3, 0.94999999999999996, "Oiseau", "admin", "Herbivore", "/images/species/ara-rouge-c3d4e5f6.jpg", true, "Ara Rouge", 1.2 },
                    { 4, 1.5, "Reptile", "admin", "Herbivore", "/images/species/tortue-geante-d4e5f6a7.jpg", true, "Tortue Géante", 250.0 },
                    { 5, 0.46000000000000002, "Mammifère", "admin", "Omnivore", "/images/species/lemur-catta-e5f6a7b8.jpg", true, "Lémur Catta", 2.2000000000000002 },
                    { 6, 85.0, "Plante", "admin", "Photosynthèse", "/images/species/sequoia-geant-f6a7b8c9.jpg", true, "Séquoia Géant", 1200000.0 },
                    { 7, 0.80000000000000004, "Plante", "admin", "Photosynthèse", "/images/species/aloe-vera-a7b8c9d0.jpg", true, "Aloe vera", 15.0 },
                    { 8, 1.6000000000000001, "Mammifère", "admin", "Carnivore", "/images/species/loup-gris-b8c9d0e1.jpg", true, "Loup gris", 45.0 },
                    { 9, 0.59999999999999998, "Mammifère", "admin", "Omnivore", "/images/species/raton-laveur-c9d0e1f2.jpg", true, "Raton laveur", 9.0 },
                    { 10, 2.2000000000000002, "Reptile", "admin", "Carnivore", "/images/species/boa-constrictor-d0e1f2a3.jpg", true, "Boa constrictor", 13.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Species");
        }
    }
}
