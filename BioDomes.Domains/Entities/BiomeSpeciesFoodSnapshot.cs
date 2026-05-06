using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Entities;

public class BiomeSpeciesFoodSnapshot
{
    public int SpeciesId { get; init; }
    public SpeciesClassification Classification { get; init; }
    public DietType Diet { get; init; }
    public int IndividualCount { get; init; }
    public double Weight { get; init; }
}
