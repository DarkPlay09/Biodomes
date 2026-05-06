using BioDomes.Domains.Queries.Biome.Details;

namespace BioDomes.Domains.Tests.Queries.Biome.Details;

[TestFixture]
public class BiomeDetailsDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeDetailsDtoIsCreated()
    {
        var dto = new BiomeDetailsDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(0));
            Assert.That(dto.Slug, Is.Empty);
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Temperature, Is.EqualTo(0d));
            Assert.That(dto.AbsoluteHumidity, Is.EqualTo(0d));
            Assert.That(dto.State, Is.Empty);
            Assert.That(dto.UpdatedAt, Is.EqualTo(default(DateTime)));
            Assert.That(dto.SpeciesCount, Is.EqualTo(0));
            Assert.That(dto.EquipmentCount, Is.EqualTo(0));
            Assert.That(dto.Species, Is.Empty);
            Assert.That(dto.Equipments, Is.Empty);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeDetailsDtoIsInitialized()
    {
        var updatedAt = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc);

        var species = new BiomeSpeciesItemDto
        {
            SpeciesId = 1,
            Name = "Lion",
            Classification = "Mammifères",
            Diet = "Carnivore",
            Weight = 190d,
            IndividualCount = 3,
            ImagePath = "/species/lion.png"
        };

        var equipment = new BiomeEquipmentItemDto
        {
            EquipmentId = 2,
            Name = "Lampe solaire",
            ImagePath = "/equipment/lamp.png",
            ProducedElement = "Lumière",
            ConsumedElement = "Eau"
        };

        var dto = new BiomeDetailsDto
        {
            Id = 10,
            Slug = "foret-tropicale",
            Name = "Forêt tropicale",
            Temperature = 24.5d,
            AbsoluteHumidity = 12.3d,
            State = "Optimal",
            UpdatedAt = updatedAt,
            SpeciesCount = 1,
            EquipmentCount = 1,
            Species = new[] { species },
            Equipments = new[] { equipment }
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(10));
            Assert.That(dto.Slug, Is.EqualTo("foret-tropicale"));
            Assert.That(dto.Name, Is.EqualTo("Forêt tropicale"));
            Assert.That(dto.Temperature, Is.EqualTo(24.5d));
            Assert.That(dto.AbsoluteHumidity, Is.EqualTo(12.3d));
            Assert.That(dto.State, Is.EqualTo("Optimal"));
            Assert.That(dto.UpdatedAt, Is.EqualTo(updatedAt));
            Assert.That(dto.SpeciesCount, Is.EqualTo(1));
            Assert.That(dto.EquipmentCount, Is.EqualTo(1));
            Assert.That(dto.Species.Single(), Is.SameAs(species));
            Assert.That(dto.Equipments.Single(), Is.SameAs(equipment));
        });
    }
}