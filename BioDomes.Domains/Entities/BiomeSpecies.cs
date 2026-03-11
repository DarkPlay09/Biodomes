namespace BioDomes.Domains.Entities;

public class BiomeSpecies
{
    public Species Species { get; set; }
    public int IndividualCount { get; set; }

    public BiomeSpecies(Species species, int individualCount)
    {
        Species = species;
        IndividualCount = individualCount;
    }
}