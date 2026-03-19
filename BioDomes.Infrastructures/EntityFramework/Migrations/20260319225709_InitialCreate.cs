using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BioDomes.Infrastructures.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Classification = table.Column<string>(type: "TEXT", nullable: false),
                    Diet = table.Column<string>(type: "TEXT", nullable: false),
                    AdultSize = table.Column<double>(type: "REAL", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "AdultSize", "Classification", "CreatedByUserName", "Diet", "ImageUrl", "IsPublic", "Name", "Weight" },
                values: new object[,]
                {
                    { 1, 2.5, "Mammifère", "admin", "Carnivore", null, true, "Lion d'Afrique", 190.0 },
                    { 2, 9.0, "Plante", "admin", "Photosynthèse", null, true, "Monstera", 12.0 },
                    { 3, 0.94999999999999996, "Oiseau", "admin", "Herbivore", null, true, "Ara Rouge", 1.2 },
                    { 4, 1.5, "Reptile", "admin", "Herbivore", null, true, "Tortue Géante", 250.0 },
                    { 5, 0.46000000000000002, "Mammifère", "admin", "Omnivore", null, true, "Lémur Catta", 2.2000000000000002 },
                    { 6, 85.0, "Plante", "admin", "Photosynthèse", null, true, "Séquoia Géant", 1200000.0 },
                    { 7, 0.80000000000000004, "Plante", "admin", "Photosynthèse", null, true, "Aloe vera", 15.0 },
                    { 8, 1.6000000000000001, "Mammifère", "admin", "Carnivore", null, true, "Loup gris", 45.0 },
                    { 9, 0.59999999999999998, "Mammifère", "admin", "Omnivore", null, true, "Raton laveur", 9.0 },
                    { 10, 2.2000000000000002, "Reptile", "admin", "Carnivore", null, true, "Boa constrictor", 13.0 }
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
