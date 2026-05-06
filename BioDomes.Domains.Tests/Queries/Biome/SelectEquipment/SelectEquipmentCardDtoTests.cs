using BioDomes.Domains.Queries.Biome.SelectEquipment;

namespace BioDomes.Domains.Tests.Queries.Biome.SelectEquipment;

[TestFixture]
public class SelectEquipmentCardDtoTests
{
    [Test]
    public void ShouldUseDefaultValues_WhenSelectEquipmentCardDtoIsCreated()
    {
        var dto = new SelectEquipmentCardDto();

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(0));
            Assert.That(dto.Name, Is.Empty);
            Assert.That(dto.Slug, Is.Empty);
            Assert.That(dto.ImagePath, Is.Empty);
            Assert.That(dto.ProducedElement, Is.Null);
            Assert.That(dto.ConsumedElement, Is.Null);
            Assert.That(dto.IsPublicAvailable, Is.False);
            Assert.That(dto.IsAlreadyInBiome, Is.False);
        });
    }

    [Test]
    public void ShouldReadInitProperties_WhenSelectEquipmentCardDtoIsInitialized()
    {
        var dto = new SelectEquipmentCardDto
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

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(11));
            Assert.That(dto.Name, Is.EqualTo("Capteur solaire"));
            Assert.That(dto.Slug, Is.EqualTo("capteur-solaire"));
            Assert.That(dto.ImagePath, Is.EqualTo("/equipment/capteur.png"));
            Assert.That(dto.ProducedElement, Is.EqualTo("Énergie"));
            Assert.That(dto.ConsumedElement, Is.EqualTo("Lumière"));
            Assert.That(dto.IsPublicAvailable, Is.True);
            Assert.That(dto.IsAlreadyInBiome, Is.True);
        });
    }

    [Test]
    public void ShouldAllowNullElements_WhenSelectEquipmentCardDtoHasNoProducedOrConsumedElement()
    {
        var dto = new SelectEquipmentCardDto
        {
            EquipmentId = 12,
            Name = "Équipement neutre",
            Slug = "equipement-neutre",
            ImagePath = "/equipment/neutre.png"
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.EquipmentId, Is.EqualTo(12));
            Assert.That(dto.Name, Is.EqualTo("Équipement neutre"));
            Assert.That(dto.Slug, Is.EqualTo("equipement-neutre"));
            Assert.That(dto.ImagePath, Is.EqualTo("/equipment/neutre.png"));
            Assert.That(dto.ProducedElement, Is.Null);
            Assert.That(dto.ConsumedElement, Is.Null);
            Assert.That(dto.IsPublicAvailable, Is.False);
            Assert.That(dto.IsAlreadyInBiome, Is.False);
        });
    }
}