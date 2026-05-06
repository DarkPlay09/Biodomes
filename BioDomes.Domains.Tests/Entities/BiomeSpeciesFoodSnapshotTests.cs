using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Tests.Entities;

[TestFixture]
public class BiomeSpeciesFoodSnapshotTests
{
    [Test]
    public void ShouldReadInitProperties_WhenUsingBiomeSpeciesFoodSnapshot()
    {
        var snapshot = new BiomeSpeciesFoodSnapshot
        {
            SpeciesId = 7,
            Classification = SpeciesClassification.Insectes,
            Diet = DietType.Herbivore,
            IndividualCount = 30,
            Weight = 0.02d
        };

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.SpeciesId, Is.EqualTo(7));
            Assert.That(snapshot.Classification, Is.EqualTo(SpeciesClassification.Insectes));
            Assert.That(snapshot.Diet, Is.EqualTo(DietType.Herbivore));
            Assert.That(snapshot.IndividualCount, Is.EqualTo(30));
            Assert.That(snapshot.Weight, Is.EqualTo(0.02d));
        });
    }
}