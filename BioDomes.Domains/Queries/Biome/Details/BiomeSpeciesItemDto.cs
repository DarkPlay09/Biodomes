namespace BioDomes.Domains.Queries;

public sealed class BiomeSpeciesItemDto
{
    public int SpeciesId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public string Diet { get; init; } = string.Empty;
    public int IndividualCount { get; init; }
}
