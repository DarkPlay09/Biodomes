using BioDomes.Domains.Queries.Biome.Details;

namespace BioDomes.Domains.Tests.Queries.Biome.Details;

[TestFixture]
public class BiomeEquipmentItemDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenBiomeEquipmentItemDtoIsCreated()
    {
        var dto = new BiomeEquipmentItemDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(0));
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.ImagePath, Is.Null);
            Assert.That(dto.ProducedElement, Is.Null);
            Assert.That(dto.ConsumedElement, Is.Null);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenBiomeEquipmentItemDtoIsInitialized()
    {
        var dto = new BiomeEquipmentItemDto
        {
            EquipmentId = 5,
            Name = "Pompe hydraulique",
            ImagePath = "/equipment/pompe.png",
            ProducedElement = "Eau",
            ConsumedElement = "Électricité"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(5));
            Assert.That(dto.Name, Is.EqualTo("Pompe hydraulique"));
            Assert.That(dto.ImagePath, Is.EqualTo("/equipment/pompe.png"));
            Assert.That(dto.ProducedElement, Is.EqualTo("Eau"));
            Assert.That(dto.ConsumedElement, Is.EqualTo("Électricité"));
        });
    }

    [Test]
    public void ShouldAllowNullOptionalElements_WhenBiomeEquipmentItemDtoIsInitializedWithoutOptionalValues()
    {
        var dto = new BiomeEquipmentItemDto
        {
            EquipmentId = 8,
            Name = "Capteur"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(8));
            Assert.That(dto.Name, Is.EqualTo("Capteur"));
            Assert.That(dto.ImagePath, Is.Null);
            Assert.That(dto.ProducedElement, Is.Null);
            Assert.That(dto.ConsumedElement, Is.Null);
        });
    }
}