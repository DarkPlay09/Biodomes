using System.Globalization;
using System.Text;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Infrastructures.Repositories;

public class EfSpeciesRepository : ISpeciesRepository
{
    private readonly BioDomesDbContext _context;

    public EfSpeciesRepository(BioDomesDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<Species> GetAll()
    {
        return _context.Species
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToList()
            .Select(ToDomain)
            .ToList();
    }

    public Species? GetBySlug(string slug)
    {
        var normalizedSlug = ToSlug(slug);

        var entity = _context.Species
            .AsNoTracking()
            .ToList()
            .FirstOrDefault(s => ToSlug(s.Name) == normalizedSlug);

        return entity is null ? null : ToDomain(entity);
    }

    public void Add(Species species)
    {
        var normalizedName = ToSlug(species.Name);

        var exists = _context.Species
            .AsNoTracking()
            .ToList()
            .Any(s => ToSlug(s.Name) == normalizedName);

        if (exists)
            throw new InvalidOperationException("Une espèce avec ce nom existe déjà.");

        _context.Species.Add(ToEntity(species));
        _context.SaveChanges();
    }

    public void Update(string slug, Species species)
    {
        var normalizedSlug = ToSlug(slug);

        var entity = _context.Species
            .ToList()
            .FirstOrDefault(s => ToSlug(s.Name) == normalizedSlug);

        if (entity is null)
            return;

        entity.Name = species.Name;
        entity.Classification = ToDbClassification(species.Classification);
        entity.Diet = ToDbDiet(species.Diet);
        entity.AdultSize = species.AdultSize;
        entity.Weight = species.Weight;
        entity.ImagePath = species.ImagePath;
        entity.CreatedByUserName = species.CreatedByUserName;
        entity.IsPublic = species.IsPublic;

        _context.SaveChanges();
    }

    public void DeleteBySlug(string slug)
    {
        var normalizedSlug = ToSlug(slug);

        var entity = _context.Species
            .ToList()
            .FirstOrDefault(s => ToSlug(s.Name) == normalizedSlug);

        if (entity is null)
            return;

        _context.Species.Remove(entity);
        _context.SaveChanges();
    }

    private static Species ToDomain(SpeciesEntity entity)
    {
        return new Species(
            entity.Name,
            ParseClassification(entity.Classification),
            ParseDiet(entity.Diet),
            entity.AdultSize,
            entity.Weight,
            entity.ImagePath,
            entity.CreatedByUserName,
            entity.IsPublic
        );
    }

    private static SpeciesEntity ToEntity(Species species)
    {
        return new SpeciesEntity
        {
            Name = species.Name,
            Classification = ToDbClassification(species.Classification),
            Diet = ToDbDiet(species.Diet),
            AdultSize = species.AdultSize,
            Weight = species.Weight,
            ImagePath = species.ImagePath,
            CreatedByUserName = species.CreatedByUserName,
            IsPublic = species.IsPublic
        };
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
            "Echinoderme" or "Échinoderme" or "Echinodermes" => SpeciesClassification.Echinodermes,
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
            SpeciesClassification.Echinodermes => "Échinoderme",
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

    private static string ToSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("'", "")
            .Replace("’", "")
            .Replace(" ", "-");
    }
}
