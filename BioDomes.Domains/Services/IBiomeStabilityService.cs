using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Services;

namespace BioDomes.Domains.Services;

public interface IBiomeStabilityService
{
    BiomeState ComputeFinalState(
        double temperature,
        double absoluteHumidity,
        IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome);

    BiomeBiomassEvaluation EvaluateBiomassBalance(IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome);

    bool IsBiomassStable(IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome);
}
