namespace BioDomes.Domains.Queries.Biome.Home;

public sealed class HomeBiomeCardDto
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;

    public double Temperature { get; init; }
    public double AbsoluteHumidity { get; init; }
    public double StabilityScore { get; init; }

    public int SpeciesCount { get; init; }
    public int EquipmentCount { get; init; }

    public DateTime UpdatedAt { get; init; }
}