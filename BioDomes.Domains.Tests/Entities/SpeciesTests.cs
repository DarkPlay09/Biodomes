using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Tests.Entities;

[TestFixture]
public class SpeciesTests
{
    private static UserAccount Creator() => new()
    {
        Id = 1,
        UserName = "chercheur",
        Email = "chercheur@biodomes.test",
        BirthDate = new DateOnly(2000, 1, 1)
    };

    [Test]
    public void ShouldInitializeProperties_WhenSpeciesReceivesValidData()
    {
        var creator = Creator();

        var species = new Species(
            name: "Panthera leo",
            classification: SpeciesClassification.Mammiferes,
            diet: DietType.Carnivore,
            adultSize: 1.2d,
            weight: 190d,
            imagePath: "/species/lion.png",
            creator: creator,
            isPublicAvailable: true);

        Assert.Multiple(() =>
        {
            Assert.That(species.Name, Is.EqualTo("Panthera leo"));
            Assert.That(species.Classification, Is.EqualTo(SpeciesClassification.Mammiferes));
            Assert.That(species.Diet, Is.EqualTo(DietType.Carnivore));
            Assert.That(species.AdultSize, Is.EqualTo(1.2d));
            Assert.That(species.Weight, Is.EqualTo(190d));
            Assert.That(species.ImagePath, Is.EqualTo("/species/lion.png"));
            Assert.That(species.Creator, Is.SameAs(creator));
            Assert.That(species.IsPublicAvailable, Is.True);
        });
    }

    [Test]
    public void ShouldUseDefaultValues_WhenOptionalValuesAreNotProvided()
    {
        var species = new Species(
            name: "Quercus robur",
            classification: SpeciesClassification.Plantes,
            diet: DietType.Photosynthese,
            adultSize: 20d,
            weight: 1000d,
            imagePath: null,
            creator: Creator());

        Assert.Multiple(() =>
        {
            Assert.That(species.ImagePath, Is.Null);
            Assert.That(species.IsPublicAvailable, Is.False);
        });
    }

    [Test]
    public void ShouldAllowPropertyUpdates_WhenPropertiesAreSet()
    {
        var creator = Creator();

        var species = new Species(
            name: "Ancien nom",
            classification: SpeciesClassification.Mammiferes,
            diet: DietType.Herbivore,
            adultSize: 1d,
            weight: 10d,
            imagePath: null,
            creator: creator);

        var newCreator = new UserAccount
        {
            Id = 2,
            UserName = "admin",
            Email = "admin@biodomes.test",
            BirthDate = new DateOnly(1999, 1, 1)
        };

        species.Id = 5;
        species.Name = "Nouveau nom";
        species.Classification = SpeciesClassification.Oiseaux;
        species.Diet = DietType.Omnivore;
        species.AdultSize = 0.5d;
        species.Weight = 3d;
        species.ImagePath = "/species/new.png";
        species.Creator = newCreator;
        species.IsPublicAvailable = true;

        Assert.Multiple(() =>
        {
            Assert.That(species.Id, Is.EqualTo(5));
            Assert.That(species.Name, Is.EqualTo("Nouveau nom"));
            Assert.That(species.Classification, Is.EqualTo(SpeciesClassification.Oiseaux));
            Assert.That(species.Diet, Is.EqualTo(DietType.Omnivore));
            Assert.That(species.AdultSize, Is.EqualTo(0.5d));
            Assert.That(species.Weight, Is.EqualTo(3d));
            Assert.That(species.ImagePath, Is.EqualTo("/species/new.png"));
            Assert.That(species.Creator, Is.SameAs(newCreator));
            Assert.That(species.IsPublicAvailable, Is.True);
        });
    }

    [Test]
    public void ShouldThrowArgumentNullException_WhenSpeciesReceivesMissingCreator()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new Species(
            name: "Panthera leo",
            classification: SpeciesClassification.Mammiferes,
            diet: DietType.Carnivore,
            adultSize: 1.2d,
            weight: 190d,
            imagePath: null,
            creator: null!));

        Assert.That(exception!.ParamName, Is.EqualTo("creator"));
    }
}