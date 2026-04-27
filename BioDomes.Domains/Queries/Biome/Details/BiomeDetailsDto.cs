namespace BioDomes.Domains.Queries;

public sealed class BiomeDetailsDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public double Temperature { get; init; }
    public double AbsoluteHumidity { get; init; }
    public string State { get; init; } = string.Empty;

    public DateTime UpdatedAt { get; init; }

    public int SpeciesCount { get; init; }
    public int EquipmentCount { get; init; }

    public IReadOnlyList<BiomeSpeciesItemDto> Species { get; init; } = [];
    public IReadOnlyList<BiomeEquipmentItemDto> Equipments { get; init; } = [];
}