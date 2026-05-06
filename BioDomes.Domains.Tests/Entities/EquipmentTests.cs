using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Tests.Entities;

[TestFixture]
public class EquipmentTests
{
    private static UserAccount Creator() => new()
    {
        Id = 1,
        UserName = "chercheur",
        Email = "chercheur@biodomes.test",
        BirthDate = new DateOnly(2000, 1, 1)
    };

    [Test]
    public void ShouldInitializeProperties_WhenEquipmentReceivesValidData()
    {
        var creator = Creator();

        var equipment = new Equipment(
            name: "Lampe solaire",
            producedElement: ResourceType.Lumiere,
            consumedElement: ResourceType.Eau,
            imagePath: "/equipment/lamp.png",
            creator: creator,
            isPublicAvailable: true);

        Assert.Multiple(() =>
        {
            Assert.That(equipment.Name, Is.EqualTo("Lampe solaire"));
            Assert.That(equipment.ProducedElement, Is.EqualTo(ResourceType.Lumiere));
            Assert.That(equipment.ConsumedElement, Is.EqualTo(ResourceType.Eau));
            Assert.That(equipment.ImagePath, Is.EqualTo("/equipment/lamp.png"));
            Assert.That(equipment.Creator, Is.SameAs(creator));
            Assert.That(equipment.IsPublicAvailable, Is.True);
        });
    }

    [Test]
    public void ShouldAcceptEquipment_WhenOnlyProducedElementIsProvided()
    {
        var equipment = new Equipment(
            name: "Générateur",
            producedElement: ResourceType.Hydrogene,
            consumedElement: null,
            imagePath: null,
            creator: Creator());

        Assert.Multiple(() =>
        {
            Assert.That(equipment.Name, Is.EqualTo("Générateur"));
            Assert.That(equipment.ProducedElement, Is.EqualTo(ResourceType.Hydrogene));
            Assert.That(equipment.ConsumedElement, Is.Null);
            Assert.That(equipment.ImagePath, Is.Null);
            Assert.That(equipment.IsPublicAvailable, Is.False);
        });
    }

    [Test]
    public void ShouldAcceptEquipment_WhenOnlyConsumedElementIsProvided()
    {
        var equipment = new Equipment(
            name: "Pompe",
            producedElement: null,
            consumedElement: ResourceType.Eau,
            imagePath: null,
            creator: Creator());

        Assert.Multiple(() =>
        {
            Assert.That(equipment.Name, Is.EqualTo("Pompe"));
            Assert.That(equipment.ProducedElement, Is.Null);
            Assert.That(equipment.ConsumedElement, Is.EqualTo(ResourceType.Eau));
            Assert.That(equipment.ImagePath, Is.Null);
            Assert.That(equipment.IsPublicAvailable, Is.False);
        });
    }

    [Test]
    public void ShouldAllowPropertyUpdates_WhenPropertiesAreSet()
    {
        var creator = Creator();

        var equipment = new Equipment(
            name: "Ancien équipement",
            producedElement: ResourceType.Lumiere,
            consumedElement: null,
            imagePath: null,
            creator: creator);

        var newCreator = new UserAccount
        {
            Id = 2,
            UserName = "admin",
            Email = "admin@biodomes.test",
            BirthDate = new DateOnly(1999, 1, 1)
        };

        equipment.Id = 8;
        equipment.Name = "Nouvel équipement";
        equipment.ProducedElement = ResourceType.Hydrogene;
        equipment.ConsumedElement = ResourceType.Eau;
        equipment.ImagePath = "/equipment/new.png";
        equipment.Creator = newCreator;
        equipment.IsPublicAvailable = true;

        Assert.Multiple(() =>
        {
            Assert.That(equipment.Id, Is.EqualTo(8));
            Assert.That(equipment.Name, Is.EqualTo("Nouvel équipement"));
            Assert.That(equipment.ProducedElement, Is.EqualTo(ResourceType.Hydrogene));
            Assert.That(equipment.ConsumedElement, Is.EqualTo(ResourceType.Eau));
            Assert.That(equipment.ImagePath, Is.EqualTo("/equipment/new.png"));
            Assert.That(equipment.Creator, Is.SameAs(newCreator));
            Assert.That(equipment.IsPublicAvailable, Is.True);
        });
    }

    [Test]
    public void ShouldThrowArgumentException_WhenEquipmentHasNoProducedOrConsumedElement()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Equipment(
            name: "Vide",
            producedElement: null,
            consumedElement: null,
            imagePath: null,
            creator: Creator()));

        Assert.That(exception!.Message, Does.Contain("produire"));
    }

    [Test]
    public void ShouldThrowArgumentNullException_WhenEquipmentReceivesMissingCreator()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new Equipment(
            name: "Lampe",
            producedElement: ResourceType.Lumiere,
            consumedElement: null,
            imagePath: null,
            creator: null!));

        Assert.That(exception!.ParamName, Is.EqualTo("creator"));
    }
}