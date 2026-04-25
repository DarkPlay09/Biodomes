using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Infrastructures.EntityFramework.Entities;

namespace BioDomes.Infrastructures.Mappers.Species;

public class SpeciesMapper : ISpeciesMapper
{
    public Domains.Entities.Species ToDomain(SpeciesEntity entity)
    {
        return new Domains.Entities.Species(
            entity.Name,
            ParseClassification(entity.Classification),
            ParseDiet(entity.Diet),
            entity.AdultSize,
            entity.Weight,
            entity.ImagePath,
            new UserAccount
            {
                Id = entity.CreatorId,
                UserName = entity.Creator?.UserName ?? string.Empty,
                Email = entity.Creator?.Email ?? string.Empty,
                AvatarPath = entity.Creator?.AvatarPath,
                BirthDate = entity.Creator?.BirthDate ?? default,
                ResearchOrganisation = entity.Creator?.ResearchOrganization,
                IsAdmin = entity.Creator?.Role == "Admin"
            },
            entity.IsPublicAvailable
        )
        {
            Id = entity.Id
        };
    }

    public SpeciesEntity ToEntity(Domains.Entities.Species species, int creatorId)
    {
        return new SpeciesEntity
        {
            Name = species.Name,
            Classification = ToDbClassification(species.Classification),
            Diet = ToDbDiet(species.Diet),
            AdultSize = species.AdultSize,
            Weight = species.Weight,
            ImagePath = species.ImagePath,
            CreatorId = creatorId,
            IsPublicAvailable = species.IsPublicAvailable
        };
    }

    public void UpdateEntity(SpeciesEntity target, Domains.Entities.Species source, int creatorId)
    {
        target.Name = source.Name;
        target.Classification = ToDbClassification(source.Classification);
        target.Diet = ToDbDiet(source.Diet);
        target.AdultSize = source.AdultSize;
        target.Weight = source.Weight;
        target.ImagePath = source.ImagePath;
        target.CreatorId = creatorId;
        target.IsPublicAvailable = source.IsPublicAvailable;
    }

    private static SpeciesClassification ParseClassification(string value)
    {
        return value.Trim() switch
        {
            "Mammifère" or "Mammifere" or "Mammiferes" => SpeciesClassification.Mammiferes,
            "Oiseau" or "Oiseaux" => SpeciesClassification.Oiseaux,
            "Poisson" or "Poissons" => SpeciesClassification.Poissons,
            "Reptile" or "Reptiles" => SpeciesClassification.Reptiles,
            "Amphibien" or "Amphibiens" => SpeciesClassification.Amphibiens,
            "Crustacé" or "Crustace" or "Crustaces" => SpeciesClassification.Crustaces,
            "Arachnide" or "Arachnides" => SpeciesClassification.Arachnides,
            "Insecte" or "Insectes" => SpeciesClassification.Insectes,
            "Myriapode" or "Myriapodes" => SpeciesClassification.Myriapodes,
            "Mollusque" or "Mollusques" => SpeciesClassification.Mollusques,
            "Echinoderme" or "Echinoderme" or "Echinodermes" => SpeciesClassification.Echinodermes,
            "Annelide" or "Annélide" or "Annelides" => SpeciesClassification.Annelides,
            "Cnidaire" or "Cnidaires" => SpeciesClassification.Cnidaires,
            "Plathelminthe" or "Plathelminthes" => SpeciesClassification.Plathelminthes,
            "Porifere" or "Porifères" or "Poriferes" => SpeciesClassification.Poriferes,
            "Plante" or "Plantes" => SpeciesClassification.Plantes,
            "Mousse" or "Mousses" => SpeciesClassification.Mousses,
            "Algue" or "Algues" => SpeciesClassification.Algues,
            "Lichen" => SpeciesClassification.Lichen,
            "Champignon" or "Champignons" => SpeciesClassification.Champignons,
            _ when Enum.TryParse<SpeciesClassification>(value, true, out var result) => result,
            _ => throw new InvalidOperationException($"Classification inconnue : {value}")
        };
    }

    private static DietType ParseDiet(string value)
    {
        return value.Trim() switch
        {
            "Carnivore" => DietType.Carnivore,
            "Herbivore" => DietType.Herbivore,
            "Omnivore" => DietType.Omnivore,
            "Coprophage" => DietType.Coprophage,
            "Limivore" => DietType.Limivore,
            "Detritivore" or "Détritivore" => DietType.Detritivore,
            "Photosynthese" or "Photosynthèse" => DietType.Photosynthese,
            "Heterotrophie" or "Hétérotrophie" => DietType.Heterotrophie,
            _ when Enum.TryParse<DietType>(value, true, out var result) => result,
            _ => throw new InvalidOperationException($"Régime alimentaire inconnu : {value}")
        };
    }

    private static string ToDbClassification(SpeciesClassification classification)
    {
        return classification switch
        {
            SpeciesClassification.Mammiferes => "Mammifère",
            SpeciesClassification.Oiseaux => "Oiseau",
            SpeciesClassification.Poissons => "Poisson",
            SpeciesClassification.Reptiles => "Reptile",
            SpeciesClassification.Amphibiens => "Amphibien",
            SpeciesClassification.Crustaces => "Crustacé",
            SpeciesClassification.Arachnides => "Arachnide",
            SpeciesClassification.Insectes => "Insecte",
            SpeciesClassification.Myriapodes => "Myriapode",
            SpeciesClassification.Mollusques => "Mollusque",
            SpeciesClassification.Echinodermes => "Echinoderme",
            SpeciesClassification.Annelides => "Annélide",
            SpeciesClassification.Cnidaires => "Cnidaire",
            SpeciesClassification.Plathelminthes => "Plathelminthe",
            SpeciesClassification.Poriferes => "Porifère",
            SpeciesClassification.Plantes => "Plante",
            SpeciesClassification.Mousses => "Mousse",
            SpeciesClassification.Algues => "Algue",
            SpeciesClassification.Lichen => "Lichen",
            SpeciesClassification.Champignons => "Champignon",
            _ => classification.ToString()
        };
    }

    private static string ToDbDiet(DietType diet)
    {
        return diet switch
        {
            DietType.Carnivore => "Carnivore",
            DietType.Herbivore => "Herbivore",
            DietType.Omnivore => "Omnivore",
            DietType.Coprophage => "Coprophage",
            DietType.Limivore => "Limivore",
            DietType.Detritivore => "Détritivore",
            DietType.Photosynthese => "Photosynthèse",
            DietType.Heterotrophie => "Hétérotrophie",
            _ => diet.ToString()
        };
    }
}
