namespace BioDomes.Domains.Queries.Biome.Species;

public sealed class BiomeSpeciesManagementPageDto
{
    public int BiomeId { get; init; }
    public string BiomeName { get; init; } = string.Empty;
    public string BiomeSlug { get; init; } = string.Empty;
    public IReadOnlyList<BiomeSpeciesManagementCardDto> SpeciesCards { get; init; } = [];
}
