namespace BioDomes.Domains.Queries.Biome.SelectEquipment;

public sealed class SelectEquipmentPageDto
{
    public int BiomeId { get; init; }
    public string BiomeName { get; init; } = string.Empty;
    public string BiomeSlug { get; init; } = string.Empty;
    public IReadOnlyList<SelectEquipmentCardDto> Equipments { get; init; } = [];
}
