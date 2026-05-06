using BioDomes.Domains.Queries.Biome.SelectEquipment;

namespace BioDomes.Domains.Tests.Queries.Biome.SelectEquipment;

[TestFixture]
public class SelectEquipmentPageDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenSelectEquipmentPageDtoIsCreated()
    {
        var dto = new SelectEquipmentPageDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(0));
            Assert.That(dto.BiomeName, Is.Empty);
            Assert.That(dto.BiomeSlug, Is.Empty);
            Assert.That(dto.Equipments, Is.Empty);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenSelectEquipmentPageDtoIsInitialized()
    {
        var equipment = new SelectEquipmentCardDto
        {
            EquipmentId = 11,
            Name = "Capteur solaire",
            Slug = "capteur-solaire",
            ImagePath = "/equipment/capteur.png",
            ProducedElement = "Énergie",
            ConsumedElement = "Lumière",
            IsPublicAvailable = true,
            IsAlreadyInBiome = true
        };

        var dto = new SelectEquipmentPageDto
        {
            BiomeId = 3,
            BiomeName = "Marais",
            BiomeSlug = "marais",
            Equipments = new[] { equipment }
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(3));
            Assert.That(dto.BiomeName, Is.EqualTo("Marais"));
            Assert.That(dto.BiomeSlug, Is.EqualTo("marais"));
            Assert.That(dto.Equipments.Single(), Is.SameAs(equipment));
        });
    }
}