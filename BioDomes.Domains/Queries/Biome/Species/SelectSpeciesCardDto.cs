namespace BioDomes.Domains.Queries.Biome.Species;

public sealed class SelectSpeciesCardDto
{
    public int SpeciesId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? ImagePath { get; init; }
    public string Classification { get; init; } = "";
    public string Diet { get; init; } = "";
    public double AdultSize { get; init; }
    public double Weight { get; init; }
    public bool IsPublicAvailable { get; init; }
    public bool IsAlreadyInBiome { get; init; }
}