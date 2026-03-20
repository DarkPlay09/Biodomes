using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.EntityFramework;

public class BioDomesDbContext : DbContext
{
    public BioDomesDbContext(DbContextOptions<BioDomesDbContext> options)
        : base(options)
    {
    }

    public DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpeciesEntity>().ToTable("Species");

        modelBuilder.Entity<SpeciesEntity>().HasData(
            new SpeciesEntity
            {
                Id = 1,
                Name = "Lion d'Afrique",
                Classification = "Mammifère",
                Diet = "Carnivore",
                AdultSize = 2.5,
                Weight = 190,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3e/Lion_d%27Afrique_%28Panthera_leo%29.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 2,
                Name = "Monstera",
                Classification = "Plante",
                Diet = "Photosynthèse",
                AdultSize = 9,
                Weight = 12,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ef/Monstera_deliciosa_Leaf_2700px.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 3,
                Name = "Ara Rouge",
                Classification = "Oiseau",
                Diet = "Herbivore",
                AdultSize = 0.95,
                Weight = 1.2,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5f/Scarlet_macaw_ara_macao.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 4,
                Name = "Tortue Géante",
                Classification = "Reptile",
                Diet = "Herbivore",
                AdultSize = 1.5,
                Weight = 250,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/16/Giant_Tortoise.JPG",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 5,
                Name = "Lémur Catta",
                Classification = "Mammifère",
                Diet = "Omnivore",
                AdultSize = 0.46,
                Weight = 2.2,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/90/Katta_Lemur_catta.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 6,
                Name = "Séquoia Géant",
                Classification = "Plante",
                Diet = "Photosynthèse",
                AdultSize = 85,
                Weight = 1200000,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/18/The_giant_sequoia_trees.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 7,
                Name = "Aloe vera",
                Classification = "Plante",
                Diet = "Photosynthèse",
                AdultSize = 0.8,
                Weight = 15,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/07/Aloe_vera_plant_in_flower_pot.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 8,
                Name = "Loup gris",
                Classification = "Mammifère",
                Diet = "Carnivore",
                AdultSize = 1.6,
                Weight = 45,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/e/e8/Loup_gris_%28Canis_lupus_%29.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 9,
                Name = "Raton laveur",
                Classification = "Mammifère",
                Diet = "Omnivore",
                AdultSize = 0.6,
                Weight = 9,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/f/fb/Raccoon_%28Procyon_lotor%29%2C_portrait.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            },
            new SpeciesEntity
            {
                Id = 10,
                Name = "Boa constrictor",
                Classification = "Reptile",
                Diet = "Carnivore",
                AdultSize = 2.2,
                Weight = 13,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ed/Boa_constrictor%2C_boa_constrictora.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}
