using BioDomes.Domains.Queries.Biome.Details;

namespace BioDomes.Domains.Tests.Queries.Biome.Details;

[TestFixture]
public class BiomeSpeciesItemDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeSpeciesItemDtoIsCreated()
    {
        var dto = new BiomeSpeciesItemDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(0));
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Classification, Is.Empty);
            Assert.That(dto.Diet, Is.Empty);
            Assert.That(dto.Weight, Is.EqualTo(0d));
            Assert.That(dto.IndividualCount, Is.EqualTo(0));
            Assert.That(dto.ImagePath, Is.Null);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeSpeciesItemDtoIsInitialized()
    {
        var dto = new BiomeSpeciesItemDto
        {
            SpeciesId = 7,
            Name = "Panthera leo",
            Classification = "Mammifères",
            Diet = "Carnivore",
            Weight = 190d,
            IndividualCount = 4,
            ImagePath = "/species/lion.png"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(7));
            Assert.That(dto.Name, Is.EqualTo("Panthera leo"));
            Assert.That(dto.Classification, Is.EqualTo("Mammifères"));
            Assert.That(dto.Diet, Is.EqualTo("Carnivore"));
            Assert.That(dto.Weight, Is.EqualTo(190d));
            Assert.That(dto.IndividualCount, Is.EqualTo(4));
            Assert.That(dto.ImagePath, Is.EqualTo("/species/lion.png"));
        });
    }

    [Test]
    public void ShouldAllowNullImagePath_WhenBiomeSpeciesItemDtoIsInitializedWithoutImage()
    {
        var dto = new BiomeSpeciesItemDto
        {
            SpeciesId = 9,
            Name = "Quercus robur",
            Classification = "Plantes",
            Diet = "Photosynthèse",
            Weight = 1000d,
            IndividualCount = 2
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.SpeciesId, Is.EqualTo(9));
            Assert.That(dto.Name, Is.EqualTo("Quercus robur"));
            Assert.That(dto.Classification, Is.EqualTo("Plantes"));
            Assert.That(dto.Diet, Is.EqualTo("Photosynthèse"));
            Assert.That(dto.Weight, Is.EqualTo(1000d));
            Assert.That(dto.IndividualCount, Is.EqualTo(2));
            Assert.That(dto.ImagePath, Is.Null);
        });
    }
}