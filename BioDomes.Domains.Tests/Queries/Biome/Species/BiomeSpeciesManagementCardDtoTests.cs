using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Tests.Queries.Biome.Species;

[TestFixture]
public class BiomeSpeciesManagementCardDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeSpeciesManagementCardDtoIsCreated()
    {
        var dto = new BiomeSpeciesManagementCardDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(0));
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Slug, Is.Empty);
            Assert.That(dto.ImagePath, Is.Null);
            Assert.That(dto.Classification, Is.Empty);
            Assert.That(dto.Diet, Is.Empty);
            Assert.That(dto.AdultSize, Is.EqualTo(0d));
            Assert.That(dto.Weight, Is.EqualTo(0d));
            Assert.That(dto.CurrentIndividualCount, Is.EqualTo(0));
            Assert.That(dto.IsPublicAvailable, Is.False);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeSpeciesManagementCardDtoIsInitialized()
    {
        var dto = new BiomeSpeciesManagementCardDto
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

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(21));
            Assert.That(dto.Name, Is.EqualTo("Grenouille verte"));
            Assert.That(dto.Slug, Is.EqualTo("grenouille-verte"));
            Assert.That(dto.ImagePath, Is.EqualTo("/species/grenouille.png"));
            Assert.That(dto.Classification, Is.EqualTo("Amphibiens"));
            Assert.That(dto.Diet, Is.EqualTo("Carnivore"));
            Assert.That(dto.AdultSize, Is.EqualTo(0.08d));
            Assert.That(dto.Weight, Is.EqualTo(0.02d));
            Assert.That(dto.CurrentIndividualCount, Is.EqualTo(12));
            Assert.That(dto.IsPublicAvailable, Is.True);
        });
    }

    [Test]
    public void ShouldAllowNullImagePath_WhenBiomeSpeciesManagementCardDtoHasNoImage()
    {
        var dto = new BiomeSpeciesManagementCardDto
        {
            SpeciesId = 24,
            Name = "Mousse",
            Slug = "mousse",
            Classification = "Plantes",
            Diet = "Photosynthèse",
            AdultSize = 0.02d,
            Weight = 0.001d,
            CurrentIndividualCount = 150
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(24));
            Assert.That(dto.Name, Is.EqualTo("Mousse"));
            Assert.That(dto.Slug, Is.EqualTo("mousse"));
            Assert.That(dto.ImagePath, Is.Null);
            Assert.That(dto.Classification, Is.EqualTo("Plantes"));
            Assert.That(dto.Diet, Is.EqualTo("Photosynthèse"));
            Assert.That(dto.AdultSize, Is.EqualTo(0.02d));
            Assert.That(dto.Weight, Is.EqualTo(0.001d));
            Assert.That(dto.CurrentIndividualCount, Is.EqualTo(150));
            Assert.That(dto.IsPublicAvailable, Is.False);
        });
    }
}