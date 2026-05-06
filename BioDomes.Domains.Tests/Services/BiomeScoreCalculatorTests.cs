using BioDomes.Domains.Services;

namespace BioDomes.Domains.Tests.Services;

[TestFixture]
public class BiomeScoreCalculatorTests
{
    [TestCase("Optimal", 100)]
    [TestCase("optimal", 100)]
    [TestCase("OPTIMAL", 100)]
    [TestCase("Instable", 60)]
    [TestCase("instable", 60)]
    [TestCase("Critique", 25)]
    [TestCase("critique", 25)]
    [TestCase(null, 50)]
    [TestCase("", 50)]
    [TestCase("Inconnu", 50)]
    public void ShouldReturnExpectedStateScore_WhenStateIsProvided(string? state, int expectedScore)
    {
        var result = BiomeScoreCalculator.GetStateScore(state);

        Assert.That(result, Is.EqualTo(expectedScore));
    }

    [Test]
    public void ShouldReturnBaseTrophicScore_WhenNoDietIsProvided()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(Array.Empty<string?>());

        Assert.That(result, Is.EqualTo(20d));
    }

    [Test]
    public void ShouldReturnBaseTrophicScore_WhenDietIsNull()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new string?[] { null });

        Assert.That(result, Is.EqualTo(20d));
    }

    [TestCase("Photosynthèse")]
    [TestCase("Autotrophe")]
    [TestCase("Producteur")]
    [TestCase("organisme photoautotrophe")]
    public void ShouldAddProducerScore_WhenDietContainsProducerKeyword(string diet)
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[] { diet });

        Assert.That(result, Is.EqualTo(50d));
    }

    [Test]
    public void ShouldAddHerbivoreScore_WhenDietContainsHerbivoreKeyword()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[] { "Herbivore" });

        Assert.That(result, Is.EqualTo(45d));
    }

    [Test]
    public void ShouldAddCarnivoreScore_WhenDietContainsCarnivoreKeyword()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[] { "Carnivore" });

        Assert.That(result, Is.EqualTo(45d));
    }

    [Test]
    public void ShouldAddHerbivoreAndCarnivoreScores_WhenDietContainsOmnivoreKeyword()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[] { "Omnivore" });

        Assert.That(result, Is.EqualTo(70d));
    }

    [Test]
    public void ShouldReturnCompleteTrophicScore_WhenProducerHerbivoreAndCarnivoreArePresent()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[]
        {
            "Photosynthèse",
            "Herbivore",
            "Carnivore"
        });

        Assert.That(result, Is.EqualTo(100d));
    }

    [Test]
    public void ShouldNotExceedMaximumTrophicScore_WhenAllDietTypesArePresent()
    {
        var result = BiomeScoreCalculator.CalculateTrophicStability(new[]
        {
            "Photosynthèse",
            "Herbivore",
            "Carnivore",
            "Omnivore"
        });

        Assert.That(result, Is.EqualTo(100d));
    }

    [Test]
    public void ShouldCalculateRoundedScore_WhenBiomeDataIsProvided()
    {
        var result = BiomeScoreCalculator.Calculate(
            state: "Optimal",
            diets: new[] { "Photosynthèse", "Herbivore" },
            speciesCount: 2,
            equipmentCount: 1);

        Assert.That(result, Is.EqualTo(92.2d));
    }

    [Test]
    public void ShouldLimitSpeciesAndEquipmentBonuses_WhenCountsAreHigh()
    {
        var result = BiomeScoreCalculator.Calculate(
            state: "Optimal",
            diets: new[] { "Photosynthèse", "Herbivore", "Carnivore" },
            speciesCount: 100,
            equipmentCount: 100);

        Assert.That(result, Is.EqualTo(100d));
    }

    [Test]
    public void ShouldReturnMaximumScore_WhenCalculatedScoreExceedsOneHundred()
    {
        var result = BiomeScoreCalculator.Calculate(
            state: "Optimal",
            diets: new[] { "Photosynthèse", "Herbivore", "Carnivore", "Omnivore" },
            speciesCount: 10,
            equipmentCount: 10);

        Assert.That(result, Is.EqualTo(100d));
    }

    [Test]
    public void ShouldUseFallbackStateScore_WhenStateIsUnknown()
    {
        var result = BiomeScoreCalculator.Calculate(
            state: "Bizarre",
            diets: Array.Empty<string?>(),
            speciesCount: 0,
            equipmentCount: 0);

        Assert.That(result, Is.EqualTo(34.5d));
    }
}