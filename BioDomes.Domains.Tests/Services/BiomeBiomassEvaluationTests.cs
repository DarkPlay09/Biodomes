using BioDomes.Domains.Services;

namespace BioDomes.Domains.Tests.Services;

[TestFixture]
public class BiomeBiomassEvaluationTests
{
    [Test]
    public void ShouldStoreProperties_WhenBiomeBiomassShortageIsCreated()
    {
        var shortage = new BiomeBiomassShortage(
            SpeciesId: 7,
            RequiredBiomass: 12.5d,
            AvailableBiomass: 4.2d);

        Assert.Multiple(() =>
        {
            Assert.That(shortage.SpeciesId, Is.EqualTo(7));
            Assert.That(shortage.RequiredBiomass, Is.EqualTo(12.5d));
            Assert.That(shortage.AvailableBiomass, Is.EqualTo(4.2d));
        });
    }

    [Test]
    public void ShouldSupportValueEquality_WhenBiomeBiomassShortagesHaveSameValues()
    {
        var first = new BiomeBiomassShortage(7, 12.5d, 4.2d);
        var second = new BiomeBiomassShortage(7, 12.5d, 4.2d);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void ShouldStoreProperties_WhenBiomeBiomassEvaluationIsCreated()
    {
        var shortages = new[]
        {
            new BiomeBiomassShortage(7, 12.5d, 4.2d)
        };

        var evaluation = new BiomeBiomassEvaluation(
            IsStable: false,
            Shortages: shortages);

        Assert.Multiple(() =>
        {
            Assert.That(evaluation.IsStable, Is.False);
            Assert.That(evaluation.Shortages, Is.SameAs(shortages));
        });
    }

    [Test]
    public void ShouldSupportValueEquality_WhenBiomeBiomassEvaluationsHaveSameValues()
    {
        var shortages = new[]
        {
            new BiomeBiomassShortage(7, 12.5d, 4.2d)
        };

        var first = new BiomeBiomassEvaluation(false, shortages);
        var second = new BiomeBiomassEvaluation(false, shortages);

        Assert.That(first, Is.EqualTo(second));
    }
}