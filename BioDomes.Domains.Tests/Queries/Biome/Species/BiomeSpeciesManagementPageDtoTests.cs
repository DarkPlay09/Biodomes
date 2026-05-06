using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Tests.Queries.Biome.Species;

[TestFixture]
public class BiomeSpeciesManagementPageDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeSpeciesManagementPageDtoIsCreated()
    {
        var dto = new BiomeSpeciesManagementPageDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(0));
            Assert.That(dto.BiomeName, Is.Empty);
            Assert.That(dto.BiomeSlug, Is.Empty);
            Assert.That(dto.SpeciesCards, Is.Empty);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeSpeciesManagementPageDtoIsInitialized()
    {
        var speciesCard = new BiomeSpeciesManagementCardDto
        {
            SpeciesId = 21,
            Name = "Grenouille verte",
            Slug = "grenouille-verte",
            ImagePath = "/species/grenouille.png",
            Classification = "Amphibiens",
            Diet = "Carnivore",
            AdultSize = 0.08d,
            Weight = 0.02d,
            CurrentIndividualCount = 12,
            IsPublicAvailable = true
        };

        var dto = new BiomeSpeciesManagementPageDto
        {
            BiomeId = 4,
            BiomeName = "Étang",
            BiomeSlug = "etang",
            SpeciesCards = new[] { speciesCard }
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(4));
            Assert.That(dto.BiomeName, Is.EqualTo("Étang"));
            Assert.That(dto.BiomeSlug, Is.EqualTo("etang"));
            Assert.That(dto.SpeciesCards.Single(), Is.SameAs(speciesCard));
        });
    }
}