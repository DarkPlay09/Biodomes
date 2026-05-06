using System.Reflection;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Services;

namespace BioDomes.Domains.Tests.Services;

[TestFixture]
public class BiomeStabilityServiceTests
{
    private readonly BiomeStabilityService _service = new();

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

    [TestCase(-21d, BiomeState.Critique)]
    [TestCase(-20d, BiomeState.Instable)]
    [TestCase(-1d, BiomeState.Instable)]
    [TestCase(0d, BiomeState.Optimal)]
    [TestCase(20d, BiomeState.Optimal)]
    [TestCase(30d, BiomeState.Optimal)]
    [TestCase(31d, BiomeState.Instable)]
    [TestCase(40d, BiomeState.Instable)]
    [TestCase(41d, BiomeState.Critique)]
    public void ShouldReturnThermalState_WhenBiomassIsStable(double temperature, BiomeState expectedState)
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 10,
                weight: 2d)
        };

        var result = _service.ComputeFinalState(
            temperature: temperature,
            absoluteHumidity: 0d,
            speciesInBiome: species);

        Assert.That(result, Is.EqualTo(expectedState));
    }

    [TestCase(20d, BiomeState.Instable)]
    [TestCase(-1d, BiomeState.Critique)]
    [TestCase(41d, BiomeState.Critique)]
    public void ShouldDegradeThermalStateByOneStep_WhenBiomassIsUnstable(double temperature, BiomeState expectedState)
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 10d)
        };

        var result = _service.ComputeFinalState(
            temperature: temperature,
            absoluteHumidity: 0d,
            speciesInBiome: species);

        Assert.That(result, Is.EqualTo(expectedState));
    }

    [Test]
    public void ShouldReturnTrue_WhenBiomassHasNoShortage()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 10,
                weight: 2d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 3d)
        };

        var result = _service.IsBiomassStable(species);

        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldReturnFalse_WhenBiomassHasShortage()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 3d)
        };

        var result = _service.IsBiomassStable(species);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldReturnStableEvaluation_WhenThereAreNoSpecies()
    {
        var result = _service.EvaluateBiomassBalance(Array.Empty<BiomeSpeciesFoodSnapshot>());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldIgnoreSpecies_WhenIndividualCountIsZero()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 0,
                weight: 10d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldSkipConsumer_WhenRequiredBiomassIsZero()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 0d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldBeStable_WhenHerbivoreHasEnoughPlantBiomass()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 10d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 5d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldReturnShortage_WhenHerbivoreHasInsufficientPlantBiomass()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 2d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 5d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.False);
            Assert.That(result.Shortages, Has.Count.EqualTo(1));
            Assert.That(result.Shortages[0].SpeciesId, Is.EqualTo(2));
            Assert.That(result.Shortages[0].RequiredBiomass, Is.EqualTo(5d));
            Assert.That(result.Shortages[0].AvailableBiomass, Is.EqualTo(2d));
        });
    }

    [Test]
    public void ShouldBeStable_WhenCarnivoreHasAnimalPrey()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Carnivore,
                count: 1,
                weight: 4d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Oiseaux,
                diet: DietType.Herbivore,
                count: 1,
                weight: 5d),
            Snapshot(
                id: 3,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 100d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.That(result.IsStable, Is.True);
    }

    [Test]
    public void ShouldExcludeSelfAndPlants_WhenCarnivoreSearchesForPrey()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Carnivore,
                count: 1,
                weight: 4d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 100d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.False);
            Assert.That(result.Shortages.Single().SpeciesId, Is.EqualTo(1));
            Assert.That(result.Shortages.Single().AvailableBiomass, Is.EqualTo(0d));
        });
    }

    [Test]
    public void ShouldBeStable_WhenOmnivoreHasOtherSpeciesAsFood()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Omnivore,
                count: 1,
                weight: 4d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 5d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.That(result.IsStable, Is.True);
    }

    [Test]
    public void ShouldExcludeSelf_WhenOmnivoreSearchesForFood()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Omnivore,
                count: 1,
                weight: 4d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.False);
            Assert.That(result.Shortages.Single().SpeciesId, Is.EqualTo(1));
            Assert.That(result.Shortages.Single().AvailableBiomass, Is.EqualTo(0d));
        });
    }

    [Test]
    public void ShouldNotCreateShortage_WhenSpeciesAreNotConsumers()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 10d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Champignons,
                diet: DietType.Detritivore,
                count: 1,
                weight: 10d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldConsumeLargestPreyStockFirst_WhenSeveralPreyStocksAreAvailable()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 3d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 10d),
            Snapshot(
                id: 3,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 5d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.That(result.IsStable, Is.True);
    }

    [Test]
    public void ShouldStopConsuming_WhenRemainingNeedIsSatisfied()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 10d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 1d),
            Snapshot(
                id: 3,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 5d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.That(result.IsStable, Is.True);
    }

    [Test]
    public void ShouldCreateShortageForSecondConsumer_WhenFirstConsumerUsesAvailableBiomass()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 5d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 4d),
            Snapshot(
                id: 3,
                classification: SpeciesClassification.Oiseaux,
                diet: DietType.Herbivore,
                count: 1,
                weight: 4d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.False);
            Assert.That(result.Shortages, Has.Count.EqualTo(1));
            Assert.That(result.Shortages[0].SpeciesId, Is.EqualTo(3));
            Assert.That(result.Shortages[0].RequiredBiomass, Is.EqualTo(4d));
            Assert.That(result.Shortages[0].AvailableBiomass, Is.EqualTo(1d));
        });
    }

    [Test]
    public void ShouldEvaluateConsumersWithFewerPreyCandidatesFirst_WhenSeveralConsumersExist()
    {
        var species = new[]
        {
            Snapshot(
                id: 1,
                classification: SpeciesClassification.Plantes,
                diet: DietType.Photosynthese,
                count: 1,
                weight: 10d),
            Snapshot(
                id: 2,
                classification: SpeciesClassification.Insectes,
                diet: DietType.Herbivore,
                count: 1,
                weight: 3d),
            Snapshot(
                id: 3,
                classification: SpeciesClassification.Mammiferes,
                diet: DietType.Omnivore,
                count: 1,
                weight: 6d)
        };

        var result = _service.EvaluateBiomassBalance(species);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsStable, Is.True);
            Assert.That(result.Shortages, Is.Empty);
        });
    }

    [Test]
    public void ShouldReturnEmptySequence_WhenUnsupportedDietIsUsedInGetPreyCandidates()
    {
        var method = typeof(BiomeStabilityService).GetMethod(
            "GetPreyCandidates",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(method, Is.Not.Null);

        var consumer = Snapshot(
            id: 1,
            classification: SpeciesClassification.Champignons,
            diet: DietType.Detritivore);

        var allSpecies = new[] { consumer };

        var result = (IEnumerable<BiomeSpeciesFoodSnapshot>)method!.Invoke(
            null,
            new object[] { consumer, allSpecies })!;

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ShouldReturnZeroBiomass_WhenPrivateComputeSpeciesBiomassReceivesInactiveSpecies()
    {
        var method = typeof(BiomeStabilityService).GetMethod(
            "ComputeSpeciesBiomass",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(method, Is.Not.Null);

        var species = Snapshot(
            id: 1,
            classification: SpeciesClassification.Mammiferes,
            diet: DietType.Herbivore,
            count: 0,
            weight: 10d);

        var result = (double)method!.Invoke(null, new object[] { species })!;

        Assert.That(result, Is.EqualTo(0d));
    }
}