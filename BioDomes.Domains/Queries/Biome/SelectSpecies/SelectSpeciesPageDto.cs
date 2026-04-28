using BioDomes.Domains.Queries.Biome.Species;

namespace BioDomes.Domains.Queries.Biome.SelectSpecies;

public sealed class SelectSpeciesPageDto
{
    public int BiomeId { get; init; }
    public string BiomeName { get; init; } = "";
    public string BiomeSlug { get; init; } = "";
    public IReadOnlyList<SelectSpeciesCardDto> Species { get; init; } = [];
}