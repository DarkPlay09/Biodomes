using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.EntityFramework.Links;

public class BiomeEquipmentLink
{
    public int BiomeId { get; set; }
    public BiomeEntity Biome { get; set; } = null!;

    public int EquipmentId { get; set; }
    public EquipmentEntity Equipment { get; set; } = null!;
}
