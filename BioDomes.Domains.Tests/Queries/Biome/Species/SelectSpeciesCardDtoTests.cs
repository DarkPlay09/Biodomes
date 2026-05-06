using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Tests.Queries.Biome.Species;

[TestFixture]
public class SelectSpeciesCardDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenSelectSpeciesCardDtoIsCreated()
    {
        var dto = new SelectSpeciesCardDto();

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
            Assert.That(dto.IsPublicAvailable, Is.False);
            Assert.That(dto.IsAlreadyInBiome, Is.False);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenSelectSpeciesCardDtoIsInitialized()
    {
        var dto = new SelectSpeciesCardDto
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

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(22));
            Assert.That(dto.Name, Is.EqualTo("Algue verte"));
            Assert.That(dto.Slug, Is.EqualTo("algue-verte"));
            Assert.That(dto.ImagePath, Is.EqualTo("/species/algue.png"));
            Assert.That(dto.Classification, Is.EqualTo("Algues"));
            Assert.That(dto.Diet, Is.EqualTo("Photosynthèse"));
            Assert.That(dto.AdultSize, Is.EqualTo(0.01d));
            Assert.That(dto.Weight, Is.EqualTo(0.001d));
            Assert.That(dto.IsPublicAvailable, Is.True);
            Assert.That(dto.IsAlreadyInBiome, Is.True);
        });
    }

    [Test]
    public void ShouldAllowNullImagePath_WhenSelectSpeciesCardDtoHasNoImage()
    {
        var dto = new SelectSpeciesCardDto
        {
            SpeciesId = 23,
            Name = "Chêne",
            Slug = "chene",
            Classification = "Plantes",
            Diet = "Photosynthèse",
            AdultSize = 20d,
            Weight = 1000d
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(23));
            Assert.That(dto.Name, Is.EqualTo("Chêne"));
            Assert.That(dto.Slug, Is.EqualTo("chene"));
            Assert.That(dto.ImagePath, Is.Null);
            Assert.That(dto.Classification, Is.EqualTo("Plantes"));
            Assert.That(dto.Diet, Is.EqualTo("Photosynthèse"));
            Assert.That(dto.AdultSize, Is.EqualTo(20d));
            Assert.That(dto.Weight, Is.EqualTo(1000d));
            Assert.That(dto.IsPublicAvailable, Is.False);
            Assert.That(dto.IsAlreadyInBiome, Is.False);
        });
    }
}