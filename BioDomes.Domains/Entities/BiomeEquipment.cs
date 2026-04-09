namespace BioDomes.Domains.Entities;

public class BiomeEquipment(Biome biome, Equipment equipment)
{
    public Biome Biome { get; } = biome ?? throw new ArgumentNullException(nameof(biome));
    public Equipment Equipment { get; } = equipment ?? throw new ArgumentNullException(nameof(equipment));
}
