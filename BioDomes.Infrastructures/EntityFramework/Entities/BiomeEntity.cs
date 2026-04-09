using BioDomes.Infrastructures.EntityFramework.Links;

namespace BioDomes.Infrastructures.EntityFramework.Entities;

public class BiomeEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double AbsoluteHumidity { get; set; }

    public string State { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int CreatorId { get; set; }
    public UserEntity Creator { get; set; } = null!;

    public ICollection<BiomeSpeciesLink> BiomeSpeciesLinks { get; set; } = new List<BiomeSpeciesLink>();
    public ICollection<BiomeEquipmentLink> BiomeEquipmentLinks { get; set; } = new List<BiomeEquipmentLink>();
}
