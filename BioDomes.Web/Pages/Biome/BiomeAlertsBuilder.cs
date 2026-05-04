using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Queries.Biome.Details;
using BioDomes.Domains.Services;

namespace BioDomes.Web.Pages.Biome;

internal static class BiomeAlertsBuilder
{
    public static IReadOnlyList<string> BuildDetailsAlerts(
        BiomeDetailsDto details,
        IBiomeStabilityService stabilityService)
    {
        var alerts = new List<string>();
        var snapshots = ToSnapshots(details.Species);

        var thermalState = ComputeThermalState(details.Temperature, details.AbsoluteHumidity);
        if (!string.Equals(thermalState, "Optimal", StringComparison.OrdinalIgnoreCase))
        {
            alerts.Add("Conditions thermiques dégradées: température et/ou humidité hors zone optimale.");
        }

        if (HasHungrySpecies(snapshots))
        {
            alerts.Add("Déséquilibre alimentaire: au moins une espèce ne trouve pas de nourriture.");
        }

        var biomass = stabilityService.EvaluateBiomassBalance(snapshots);
        if (!biomass.IsStable)
        {
            alerts.Add("Biomasse insuffisante pour certaines espèces:");

            var namesById = details.Species
                .ToDictionary(s => s.SpeciesId, s => s.Name);

            foreach (var shortage in biomass.Shortages)
            {
                namesById.TryGetValue(shortage.SpeciesId, out var speciesName);
                speciesName ??= $"Espèce {shortage.SpeciesId}";
                var missing = Math.Max(0d, shortage.RequiredBiomass - shortage.AvailableBiomass);
                alerts.Add($"- {speciesName}: manque {missing:0.##} kg de biomasse.");
            }
        }

        return alerts;
    }

    public static IReadOnlyList<string> BuildSpeciesAlerts(
        IReadOnlyList<BiomeSpeciesItemDto> species,
        IBiomeStabilityService stabilityService)
    {
        var alerts = new List<string>();
        var snapshots = ToSnapshots(species);

        if (HasHungrySpecies(snapshots))
        {
            alerts.Add("Déséquilibre alimentaire: au moins une espèce ne trouve pas de nourriture.");
        }

        var biomass = stabilityService.EvaluateBiomassBalance(snapshots);
        if (!biomass.IsStable)
        {
            alerts.Add("Biomasse insuffisante pour certaines espèces:");

            var namesById = species.ToDictionary(s => s.SpeciesId, s => s.Name);

            foreach (var shortage in biomass.Shortages)
            {
                namesById.TryGetValue(shortage.SpeciesId, out var speciesName);
                speciesName ??= $"Espèce {shortage.SpeciesId}";
                var missing = Math.Max(0d, shortage.RequiredBiomass - shortage.AvailableBiomass);
                alerts.Add($"- {speciesName}: manque {missing:0.##} kg de biomasse.");
            }
        }

        return alerts;
    }

    private static List<BiomeSpeciesFoodSnapshot> ToSnapshots(IReadOnlyList<BiomeSpeciesItemDto> species)
    {
        return species
            .Select(s => new BiomeSpeciesFoodSnapshot
            {
                SpeciesId = s.SpeciesId,
                Classification = ParseClassification(s.Classification),
                Diet = ParseDiet(s.Diet),
                IndividualCount = s.IndividualCount,
                Weight = s.Weight
            })
            .ToList();
    }

    private static bool HasHungrySpecies(IReadOnlyList<BiomeSpeciesFoodSnapshot> speciesInBiome)
    {
        foreach (var current in speciesInBiome)
        {
            if (current.IndividualCount <= 0) continue;
            if (current.Diet == DietType.Photosynthese) continue;

            var hasFood = current.Diet switch
            {
                DietType.Herbivore => speciesInBiome.Any(s =>
                    s.IndividualCount > 0 &&
                    s.Classification == SpeciesClassification.Plantes),

                DietType.Carnivore => speciesInBiome.Any(s =>
                    s.IndividualCount > 0 &&
                    s.SpeciesId != current.SpeciesId &&
                    s.Classification != SpeciesClassification.Plantes),

                DietType.Omnivore => speciesInBiome.Any(s =>
                    s.IndividualCount > 0 &&
                    s.SpeciesId != current.SpeciesId),

                _ => true
            };

            if (!hasFood)
                return true;
        }

        return false;
    }

    private static string ComputeThermalState(double temperature, double absoluteHumidity)
    {
        switch (temperature)
        {
            case < -20d:
            case > 40d:
                return "Critique";
            case < 0d:
            case > 30d:
                return "Instable";
        }

        if (absoluteHumidity is < 0d or > 30d)
            return "Instable";

        return "Optimal";
    }

    private static SpeciesClassification ParseClassification(string rawValue)
    {
        var normalized = Normalize(rawValue);

        return normalized switch
        {
            "mammifere" => SpeciesClassification.Mammiferes,
            "oiseau" => SpeciesClassification.Oiseaux,
            "poisson" => SpeciesClassification.Poissons,
            "reptile" => SpeciesClassification.Reptiles,
            "amphibien" => SpeciesClassification.Amphibiens,
            "crustace" => SpeciesClassification.Crustaces,
            "arachnide" => SpeciesClassification.Arachnides,
            "insecte" => SpeciesClassification.Insectes,
            "mollusque" => SpeciesClassification.Mollusques,
            "plante" => SpeciesClassification.Plantes,
            "mousse" => SpeciesClassification.Mousses,
            "algue" => SpeciesClassification.Algues,
            "lichen" => SpeciesClassification.Lichen,
            "champignon" => SpeciesClassification.Champignons,
            "myriapode" => SpeciesClassification.Myriapodes,
            "echinoderme" => SpeciesClassification.Echinodermes,
            "annelide" => SpeciesClassification.Annelides,
            "cnidaire" => SpeciesClassification.Cnidaires,
            "plathelminthe" => SpeciesClassification.Plathelminthes,
            "porifere" => SpeciesClassification.Poriferes,
            _ => SpeciesClassification.Plantes
        };
    }

    private static DietType ParseDiet(string rawValue)
    {
        var normalized = Normalize(rawValue);

        if (normalized.Contains("photosynth")) return DietType.Photosynthese;
        if (normalized.Contains("carnivore")) return DietType.Carnivore;
        if (normalized.Contains("herbivore")) return DietType.Herbivore;
        if (normalized.Contains("omnivore")) return DietType.Omnivore;
        if (normalized.Contains("detritivore")) return DietType.Detritivore;
        if (normalized.Contains("heterotroph")) return DietType.Heterotrophie;

        return DietType.Omnivore;
    }

    private static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        return raw.Trim().ToLowerInvariant();
    }
}
