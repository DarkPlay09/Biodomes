namespace BioDomes.Domains.Queries.Biome.Species;

public sealed class BiomeSpeciesManagementCardDto
{
    public int SpeciesId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImagePath { get; init; }
    public string Classification { get; init; } = string.Empty;
    public string Diet { get; init; } = string.Empty;
    public double AdultSize { get; init; }
    public double Weight { get; init; }
    public int CurrentIndividualCount { get; init; }
    public bool IsPublicAvailable { get; init; }
}
