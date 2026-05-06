using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Tests.Entities;

[TestFixture]
public class BiomeTests
{
    private static UserAccount Creator() => new()
    {
        Id = 1,
        UserName = "chercheur",
        Email = "chercheur@biodomes.test",
        BirthDate = new DateOnly(2000, 1, 1)
    };

    private static Biome ValidBiome(double temperature = 20d, double humidity = 5d)
        => new("Forêt tropicale", temperature, humidity, Creator(), new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));

    private static BiomeSpeciesFoodSnapshot Snapshot(
        int id,
        SpeciesClassification classification,
        DietType diet,
        int count = 1,
        double weight = 1d)
        => new()
        {
            SpeciesId = id,
            Classification = classification,
            Diet = diet,
            IndividualCount = count,
            Weight = weight
        };

    [Test]
    public void ShouldInitializePropertiesAndComputeState_WhenConstructorReceivesValidData()
    {
        var creator = Creator();
        var updatedAt = new DateTime(2026, 4, 1, 12, 30, 0, DateTimeKind.Utc);

        var biome = new Biome("Désert", 20d, 3d, creator, updatedAt);

        Assert.Multiple(() =>
        {
            Assert.That(biome.Name, Is.EqualTo("Désert"));
            Assert.That(biome.Temperature, Is.EqualTo(20d));
            Assert.That(biome.AbsoluteHumidity, Is.EqualTo(3d));
            Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
            Assert.That(biome.Creator, Is.SameAs(creator));
            Assert.That(biome.UpdatedAt, Is.EqualTo(updatedAt));
        });
    }

    [Test]
    public void ShouldUseCurrentUtcDate_WhenConstructorReceivesNoUpdatedAt()
    {
        var before = DateTime.UtcNow;
        var biome = new Biome("Savane", 20d, 3d, Creator());
        var after = DateTime.UtcNow;

        Assert.That(biome.UpdatedAt, Is.InRange(before, after));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldThrowArgumentException_WhenConstructorReceivesMissingName(string? name)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Biome(name!, 20d, 3d, Creator()));

        Assert.That(exception!.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void ShouldThrowArgumentException_WhenConstructorReceivesMissingCreator()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Biome("Forêt", 20d, 3d, null!));

        Assert.That(exception!.ParamName, Is.EqualTo("creator"));
    }

    [Test]
    public void ShouldThrowArgumentOutOfRangeException_WhenConstructorReceivesNegativeHumidity()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Biome("Forêt", 20d, -0.1d, Creator()));

        Assert.That(exception!.ParamName, Is.EqualTo("absoluteHumidity"));
    }

    [Test]
    public void ShouldThrowArgumentOutOfRangeException_WhenConstructorReceivesTooLowTemperature()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Biome("Forêt", -100d, 0d, Creator()));

        Assert.That(exception!.ParamName, Is.EqualTo("temperature"));
    }

    [Test]
    public void ShouldThrowInvalidOperationException_WhenConstructorReceivesHumidityAbovePhysicalMaximum()
    {
        var tooHighHumidity = Biome.ComputeMaxAbsoluteHumidity(20d) + 0.01d;

        var exception = Assert.Throws<InvalidOperationException>(() => new Biome("Serre", 20d, tooHighHumidity, Creator()));

        Assert.That(exception!.Message, Does.Contain("humidité max"));
    }

    [Test]
    public void ShouldAcceptBiome_WhenHumidityEqualsPhysicalMaximum()
    {
        var maxHumidity = Biome.ComputeMaxAbsoluteHumidity(20d);

        var biome = new Biome("Serre", 20d, maxHumidity, Creator());

        Assert.That(biome.AbsoluteHumidity, Is.EqualTo(maxHumidity));
    }

    [Test]
    public void ShouldReturnExpectedApproximation_WhenComputingMaxAbsoluteHumidity()
    {
        var result = Biome.ComputeMaxAbsoluteHumidity(20d);

        Assert.That(result, Is.EqualTo(17.27d).Within(0.05d));
    }

    [TestCase(-21d, BiomeState.Critique)]
    [TestCase(-1d, BiomeState.Instable)]
    [TestCase(0d, BiomeState.Optimal)]
    [TestCase(30d, BiomeState.Optimal)]
    [TestCase(31d, BiomeState.Instable)]
    [TestCase(41d, BiomeState.Critique)]
    public void ShouldUpdateValuesAndComputeState_WhenUpdatingConditions(double temperature, BiomeState expectedState)
    {
        var biome = ValidBiome();

        biome.UpdateConditions(temperature, 0d);

        Assert.Multiple(() =>
        {
            Assert.That(biome.Temperature, Is.EqualTo(temperature));
            Assert.That(biome.AbsoluteHumidity, Is.EqualTo(0d));
            Assert.That(biome.State, Is.EqualTo(expectedState));
        });
    }

    [Test]
    public void ShouldDegradeStateStepByStep_WhenDegradeStateByOneStepIsCalled()
    {
        var biome = ValidBiome();

        biome.DegradeStateByOneStep();
        Assert.That(biome.State, Is.EqualTo(BiomeState.Instable));

        biome.DegradeStateByOneStep();
        Assert.That(biome.State, Is.EqualTo(BiomeState.Critique));

        biome.DegradeStateByOneStep();
        Assert.That(biome.State, Is.EqualTo(BiomeState.Critique));
    }

    [Test]
    public void ShouldRecomputeThermalState_WhenEcosystemHasNoHungrySpecies()
    {
        var biome = ValidBiome();
        biome.DegradeStateByOneStep();

        biome.RecomputeStateFromEcosystem(Array.Empty<BiomeSpeciesFoodSnapshot>());

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }

    [Test]
    public void ShouldNotDegradeState_WhenSpeciesAreInactive()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Carnivore, count: 0),
            Snapshot(2, SpeciesClassification.Plantes, DietType.Photosynthese)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }

    [Test]
    public void ShouldNotDegradeState_WhenHerbivoreHasPlantFood()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Herbivore),
            Snapshot(2, SpeciesClassification.Plantes, DietType.Photosynthese)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }

    [Test]
    public void ShouldDegradeState_WhenHerbivoreHasNoFood()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Herbivore)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Instable));
    }

    [Test]
    public void ShouldNotDegradeState_WhenCarnivoreHasAnimalPrey()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Carnivore),
            Snapshot(2, SpeciesClassification.Oiseaux, DietType.Herbivore),
            Snapshot(3, SpeciesClassification.Plantes, DietType.Photosynthese)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }

    [Test]
    public void ShouldDegradeState_WhenCarnivoreHasNoAnimalPrey()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Carnivore),
            Snapshot(2, SpeciesClassification.Plantes, DietType.Photosynthese)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Instable));
    }

    [Test]
    public void ShouldNotDegradeState_WhenOmnivoreHasOtherSpeciesAsFood()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Omnivore),
            Snapshot(2, SpeciesClassification.Plantes, DietType.Photosynthese)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }

    [Test]
    public void ShouldDegradeState_WhenOmnivoreHasNoFood()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Mammiferes, DietType.Omnivore)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Instable));
    }

    [Test]
    public void ShouldNotDegradeState_WhenDietIsUnsupported()
    {
        var biome = ValidBiome();
        var species = new[]
        {
            Snapshot(1, SpeciesClassification.Champignons, DietType.Detritivore)
        };

        biome.RecomputeStateFromEcosystem(species);

        Assert.That(biome.State, Is.EqualTo(BiomeState.Optimal));
    }
}