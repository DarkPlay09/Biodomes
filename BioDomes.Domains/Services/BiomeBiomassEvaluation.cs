namespace BioDomes.Domains.Services;

public sealed record BiomeBiomassShortage(
    int SpeciesId,
    double RequiredBiomass,
    double AvailableBiomass);

public sealed record BiomeBiomassEvaluation(
    bool IsStable,
    IReadOnlyList<BiomeBiomassShortage> Shortages);
