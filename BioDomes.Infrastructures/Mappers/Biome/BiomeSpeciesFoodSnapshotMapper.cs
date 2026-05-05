using System.Globalization;
using System.Text;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Infrastructures.EntityFramework.Links;

namespace BioDomes.Infrastructures.Mappers.Biome;

public class BiomeSpeciesFoodSnapshotMapper : IBiomeSpeciesFoodSnapshotMapper
{
    public IReadOnlyList<BiomeSpeciesFoodSnapshot> ToFoodSnapshots(IEnumerable<BiomeSpeciesLink> speciesLinks)
    {
        return speciesLinks
            .Where(link => link.Species is not null)
            .Select(link => new BiomeSpeciesFoodSnapshot
            {
                SpeciesId = link.SpeciesId,
                Classification = ParseClassification(link.Species!.Classification),
                Diet = ParseDiet(link.Species.Diet),
                IndividualCount = link.IndividualCount,
                Weight = link.Species.Weight
            })
            .ToList();
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

        var normalized = raw.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(chars);
    }
}
