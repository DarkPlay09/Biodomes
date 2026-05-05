using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Services;

public class BiomeStabilityService : IBiomeStabilityService
{
    public BiomeState ComputeFinalState(
        double temperature,
        double absoluteHumidity,
        IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome)
    {
        var baseState = ComputeThermalState(temperature);
        var biomassEvaluation = EvaluateBiomassBalance(speciesInBiome);

        if (biomassEvaluation.IsStable)
            return baseState;

        return DegradeByOneStep(baseState);
    }

    public bool IsBiomassStable(IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome)
    {
        return EvaluateBiomassBalance(speciesInBiome).IsStable;
    }

    public BiomeBiomassEvaluation EvaluateBiomassBalance(IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome)
    {
        var activeSpecies = speciesInBiome
            .Where(s => s.IndividualCount > 0)
            .ToList();

        // Stock initial de biomasse disponible par espèce
        var remainingBiomass = activeSpecies.ToDictionary(
            s => s.SpeciesId,
            s => ComputeSpeciesBiomass(s));

        // On alimente d'abord les consommateurs les plus contraints (moins de proies possibles)
        var consumers = activeSpecies
            .Where(IsConsumer)
            .Select(s => new
            {
                Species = s,
                PreyIds = GetPreyCandidates(s, activeSpecies).Select(p => p.SpeciesId).Distinct().ToList()
            })
            .OrderBy(x => x.PreyIds.Count)
            .ThenBy(x => x.Species.SpeciesId)
            .ToList();

        var shortages = new List<BiomeBiomassShortage>();

        foreach (var consumer in consumers)
        {
            var required = ComputeSpeciesBiomass(consumer.Species);
            if (required <= 0d)
                continue;

            var available = consumer.PreyIds
                .Where(remainingBiomass.ContainsKey)
                .Sum(preyId => remainingBiomass[preyId]);

            if (available + 1e-9 < required)
            {
                shortages.Add(new BiomeBiomassShortage(
                    consumer.Species.SpeciesId,
                    required,
                    available));
                continue;
            }

            ConsumeBiomass(required, consumer.PreyIds, remainingBiomass);
        }

        return new BiomeBiomassEvaluation(
            IsStable: shortages.Count == 0,
            Shortages: shortages);
    }

    private static BiomeState ComputeThermalState(double temperature)
    {
        return temperature switch
        {
            < -20 or > 40 => BiomeState.Critique,
            < 0 or > 30 => BiomeState.Instable,
            _ => BiomeState.Optimal
        };
    }

    private static BiomeState DegradeByOneStep(BiomeState currentState)
    {
        return currentState switch
        {
            BiomeState.Optimal => BiomeState.Instable,
            BiomeState.Instable => BiomeState.Critique,
            _ => BiomeState.Critique
        };
    }

    private static bool IsConsumer(BiomeSpeciesFoodSnapshot snapshot)
    {
        return snapshot.Diet is DietType.Herbivore or DietType.Carnivore or DietType.Omnivore;
    }

    private static IEnumerable<BiomeSpeciesFoodSnapshot> GetPreyCandidates(
        BiomeSpeciesFoodSnapshot consumer,
        IReadOnlyList<BiomeSpeciesFoodSnapshot> allSpecies)
    {
        return consumer.Diet switch
        {
            DietType.Herbivore =>
                allSpecies.Where(s => s.Classification == SpeciesClassification.Plantes),

            DietType.Carnivore =>
                allSpecies.Where(s =>
                    s.SpeciesId != consumer.SpeciesId &&
                    s.Classification != SpeciesClassification.Plantes),

            DietType.Omnivore =>
                allSpecies.Where(s => s.SpeciesId != consumer.SpeciesId),

            _ => Enumerable.Empty<BiomeSpeciesFoodSnapshot>()
        };
    }

    private static double ComputeSpeciesBiomass(BiomeSpeciesFoodSnapshot species)
    {
        if (species.IndividualCount <= 0 || species.Weight <= 0d)
            return 0d;

        return species.IndividualCount * species.Weight;
    }

    private static void ConsumeBiomass(
        double required,
        IReadOnlyList<int> preyIds,
        IDictionary<int, double> remainingBiomass)
    {
        var remainingNeed = required;

        // On consomme d'abord les stocks les plus gros pour limiter la fragmentation
        var orderedPrey = preyIds
            .Where(preyId => remainingBiomass.TryGetValue(preyId, out var stock) && stock > 0d)
            .OrderByDescending(preyId => remainingBiomass[preyId])
            .ToList();

        foreach (var preyId in orderedPrey)
        {
            if (remainingNeed <= 0d)
                break;

            var stock = remainingBiomass[preyId];
            var consumed = Math.Min(stock, remainingNeed);
            remainingBiomass[preyId] = stock - consumed;
            remainingNeed -= consumed;
        }
    }
}
