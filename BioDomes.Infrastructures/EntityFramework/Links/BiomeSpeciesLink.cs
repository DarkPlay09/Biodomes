using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.EntityFramework.Links;

public class BiomeSpeciesLink
{
    public int BiomeId { get; set; }
    public BiomeEntity Biome { get; set; } = null!;

    public int SpeciesId { get; set; }
    public SpeciesEntity Species { get; set; } = null!;

    public int IndividualCount { get; set; }
}
