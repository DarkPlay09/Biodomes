namespace BioDomes.Domains.Entities;

public class BiomeSpecies
{
    public Biome Biome { get; }
    public Species Species { get; }
    public int IndividualCount { get; private set; } // nombre de la même espèce dans le Biome

    public BiomeSpecies(Biome biome, Species species, int individualCount)
    {
        if (individualCount <= 0) throw new ArgumentOutOfRangeException(nameof(individualCount));
        Biome = biome ?? throw new ArgumentNullException(nameof(biome));
        Species = species ?? throw new ArgumentNullException(nameof(species));
        IndividualCount = individualCount;
    }
}