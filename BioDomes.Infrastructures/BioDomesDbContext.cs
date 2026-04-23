using BioDomes.Domains.Enums;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.EntityFramework.Links;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures;

public class BioDomesDbContext : IdentityDbContext<UserEntity, IdentityRole<int>, int>
{
    public BioDomesDbContext(DbContextOptions<BioDomesDbContext> options)
        : base(options)
    {
    }

    public DbSet<BiomeEntity> Biomes => Set<BiomeEntity>();
    public DbSet<SpeciesEntity> Species => Set<SpeciesEntity>();
    public DbSet<EquipmentEntity> Equipments => Set<EquipmentEntity>();
    public DbSet<BiomeSpeciesLink> BiomeSpeciesLinks => Set<BiomeSpeciesLink>();
    public DbSet<BiomeEquipmentLink> BiomeEquipmentLinks => Set<BiomeEquipmentLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BioDomesDbContext).Assembly);

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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
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
                IsPublicAvailable = true,
                CreatorId = 1
            }
        );

        modelBuilder.Entity<EquipmentEntity>().HasData(
            new EquipmentEntity
            {
                Id = 1,
                Name = "Helio Lamp A7F3K2Q1",
                ImagePath = "/images/equipment/helio-lamp-a7f3k2q1.jpg",
                ProducedElement = ResourceType.Lumiere,
                ConsumedElement = ResourceType.Hydrogene,
                IsPublicAvailable = true,
                CreatorId = 1
            },
            new EquipmentEntity
            {
                Id = 2,
                Name = "UV Array B9M4D8R2",
                ImagePath = "/images/equipment/uv-array-b9m4d8r2.jpg",
                ProducedElement = ResourceType.Lumiere,
                ConsumedElement = ResourceType.Azote,
                IsPublicAvailable = true,
                CreatorId = 1
            },
            new EquipmentEntity
            {
                Id = 3,
                Name = "Micro Pump C6P1T7L5",
                ImagePath = "/images/equipment/micro-pump-c6p1t7l5.jpg",
                ProducedElement = ResourceType.Eau,
                ConsumedElement = ResourceType.Hydrogene,
                IsPublicAvailable = true,
                CreatorId = 1
            },
            new EquipmentEntity
            {
                Id = 4,
                Name = "Nitro Filter D3X8N4V6",
                ImagePath = "/images/equipment/nitro-filter-d3x8n4v6.jpg",
                ProducedElement = ResourceType.Azote,
                ConsumedElement = ResourceType.Eau,
                IsPublicAvailable = true,
                CreatorId = 1
            },
            new EquipmentEntity
            {
                Id = 5,
                Name = "Hydro Cell E2R9J5S8",
                ImagePath = "/images/equipment/hydro-cell-e2r9j5s8.jpg",
                ProducedElement = ResourceType.Hydrogene,
                ConsumedElement = ResourceType.Eau,
                IsPublicAvailable = true,
                CreatorId = 1
            }
        );
    }
}
