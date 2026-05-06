using BioDomes.Domains.Queries.Biome.SelectSpecies;
using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Tests.Queries.Biome.SelectSpecies;

[TestFixture]
public class SelectSpeciesPageDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenSelectSpeciesPageDtoIsCreated()
    {
        var dto = new SelectSpeciesPageDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(0));
            Assert.That(dto.BiomeName, Is.Empty);
            Assert.That(dto.BiomeSlug, Is.Empty);
            Assert.That(dto.Species, Is.Empty);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenSelectSpeciesPageDtoIsInitialized()
    {
        var species = new SelectSpeciesCardDto
        {
            SpeciesId = 22,
            Name = "Algue verte",
            Slug = "algue-verte",
            ImagePath = "/species/algue.png",
            Classification = "Algues",
            Diet = "Photosynthèse",
            AdultSize = 0.01d,
            Weight = 0.001d,
            IsPublicAvailable = true,
            IsAlreadyInBiome = true
        };

        var dto = new SelectSpeciesPageDto
        {
            BiomeId = 5,
            BiomeName = "Récif",
            BiomeSlug = "recif",
            Species = new[] { species }
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.BiomeId, Is.EqualTo(5));
            Assert.That(dto.BiomeName, Is.EqualTo("Récif"));
            Assert.That(dto.BiomeSlug, Is.EqualTo("recif"));
            Assert.That(dto.Species.Single(), Is.SameAs(species));
        });
    }
}