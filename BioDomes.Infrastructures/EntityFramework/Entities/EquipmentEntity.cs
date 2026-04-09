using BioDomes.Infrastructures.EntityFramework.Links;

namespace BioDomes.Infrastructures.EntityFramework.Entities;

public class EquipmentEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }

    public string? ProducedElement { get; set; }
    public string? ConsumedElement { get; set; }

    public bool IsPublicAvailable { get; set; }

    public int CreatorId { get; set; }
    public UserEntity Creator { get; set; } = null!;

    public ICollection<BiomeEquipmentLink> BiomeEquipmentLinks { get; set; } = new List<BiomeEquipmentLink>();
}
