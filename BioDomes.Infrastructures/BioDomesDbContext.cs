using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures;

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
                ImagePath = "/images/species/lion-dafrique-a1b2c3d4.jpg",
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
                ImagePath = "/images/species/monstera-b2c3d4e5.jpg",
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
                ImagePath = "/images/species/ara-rouge-c3d4e5f6.jpg",
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
                ImagePath = "/images/species/tortue-geante-d4e5f6a7.jpg",
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
                ImagePath = "/images/species/lemur-catta-e5f6a7b8.jpg",
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
                ImagePath = "/images/species/sequoia-geant-f6a7b8c9.jpg",
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
                ImagePath = "/images/species/aloe-vera-a7b8c9d0.jpg",
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
                ImagePath = "/images/species/loup-gris-b8c9d0e1.jpg",
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
                ImagePath = "/images/species/raton-laveur-c9d0e1f2.jpg",
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
                ImagePath = "/images/species/boa-constrictor-d0e1f2a3.jpg",
                IsPublic = true,
                CreatedByUserName = "admin"
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}
