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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
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
                ImageUrl = null,
                IsPublic = true,
                CreatedByUserName = "admin"
            }
        );

        base.OnModelCreating(modelBuilder);
    }
}