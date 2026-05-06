namespace BioDomes.Domains.Queries.Biome.SelectEquipment;

public sealed class SelectEquipmentCardDto
{
    public int EquipmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public string? ProducedElement { get; init; }
    public string? ConsumedElement { get; init; }
    public bool IsPublicAvailable { get; init; }
    public bool IsAlreadyInBiome { get; init; }
}
