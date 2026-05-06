using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Tests.Queries.Biome.Species;

[TestFixture]
public class BiomeSpeciesManagementFiltersDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeSpeciesManagementFiltersDtoIsCreated()
    {
        var dto = new BiomeSpeciesManagementFiltersDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.Search, Is.Null);
            Assert.That(dto.Classification, Is.Null);
            Assert.That(dto.Diet, Is.Null);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeSpeciesManagementFiltersDtoIsInitialized()
    {
        var dto = new BiomeSpeciesManagementFiltersDto
        {
            Search = "gre",
            Classification = "Amphibiens",
            Diet = "Carnivore"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Search, Is.EqualTo("gre"));
            Assert.That(dto.Classification, Is.EqualTo("Amphibiens"));
            Assert.That(dto.Diet, Is.EqualTo("Carnivore"));
        });
    }
}