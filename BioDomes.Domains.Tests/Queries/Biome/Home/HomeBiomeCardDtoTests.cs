using BioDomes.Domains.Queries.Biome.Home;

namespace BioDomes.Domains.Tests.Queries.Biome.Home;

[TestFixture]
public class HomeBiomeCardDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenHomeBiomeCardDtoIsCreated()
    {
        var dto = new HomeBiomeCardDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Slug, Is.Empty);
            Assert.That(dto.State, Is.Empty);
            Assert.That(dto.Temperature, Is.EqualTo(0d));
            Assert.That(dto.AbsoluteHumidity, Is.EqualTo(0d));
            Assert.That(dto.StabilityScore, Is.EqualTo(0d));
            Assert.That(dto.SpeciesCount, Is.EqualTo(0));
            Assert.That(dto.EquipmentCount, Is.EqualTo(0));
            Assert.That(dto.UpdatedAt, Is.EqualTo(default(DateTime)));
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenHomeBiomeCardDtoIsInitialized()
    {
        var updatedAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

        var dto = new HomeBiomeCardDto
        {
            Name = "Toundra",
            Slug = "toundra",
            State = "Instable",
            Temperature = -5d,
            AbsoluteHumidity = 1.5d,
            StabilityScore = 72.5d,
            SpeciesCount = 4,
            EquipmentCount = 2,
            UpdatedAt = updatedAt
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Name, Is.EqualTo("Toundra"));
            Assert.That(dto.Slug, Is.EqualTo("toundra"));
            Assert.That(dto.State, Is.EqualTo("Instable"));
            Assert.That(dto.Temperature, Is.EqualTo(-5d));
            Assert.That(dto.AbsoluteHumidity, Is.EqualTo(1.5d));
            Assert.That(dto.StabilityScore, Is.EqualTo(72.5d));
            Assert.That(dto.SpeciesCount, Is.EqualTo(4));
            Assert.That(dto.EquipmentCount, Is.EqualTo(2));
            Assert.That(dto.UpdatedAt, Is.EqualTo(updatedAt));
        });
    }
}