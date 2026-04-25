using System.Globalization;
using System.Security.Claims;
using System.Text;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Species;

public class DetailsModel : PageModel
{
    private readonly ISpeciesRepository _repository;

    public DetailsModel(ISpeciesRepository repository)
    {
        _repository = repository;
    }

    public SpeciesDetailsViewModel SpeciesDetails { get; private set; } = new();

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var species = _repository.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        var isOwner = species.Creator.Id == currentUserId;

        if (!species.IsPublicAvailable && !isOwner)
        {
            return Forbid();
        }

        SpeciesDetails = MapToDetails(species, isOwner);

        return Page();
    }

    private SpeciesDetailsViewModel MapToDetails(SpeciesEntity species, bool isOwner)
    {
        return new SpeciesDetailsViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = ToSlug(species.Name),
            ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                ? "/images/species/noImageSpecie.png"
                : species.ImagePath,
            ClassificationLabel = FormatClassification(species.Classification),
            DietLabel = FormatDiet(species.Diet),
            AdultSizeLabel = FormatSize(species.AdultSize),
            WeightLabel = FormatWeight(species.Weight),
            VisibilityLabel = species.IsPublicAvailable ? "Publique" : "Privée",
            VisibilityCssClass = species.IsPublicAvailable
                ? "species-details-badge--public"
                : "species-details-badge--private",
            OwnerName = string.IsNullOrWhiteSpace(species.Creator.UserName)
                ? "Créateur inconnu"
                : species.Creator.UserName,
            IsOwner = isOwner
        };
    }

    public string FormatClassification(SpeciesClassification classification)
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
            SpeciesClassification.Mollusques => "Mollusque",
            SpeciesClassification.Plantes => "Plante",
            SpeciesClassification.Mousses => "Mousse",
            SpeciesClassification.Algues => "Algue",
            SpeciesClassification.Lichen => "Lichen",
            SpeciesClassification.Champignons => "Champignon",
            SpeciesClassification.Myriapodes => "Myriapode",
            SpeciesClassification.Echinodermes => "Échinoderme",
            SpeciesClassification.Annelides => "Annélide",
            SpeciesClassification.Cnidaires => "Cnidaire",
            SpeciesClassification.Plathelminthes => "Plathelminthe",
            SpeciesClassification.Poriferes => "Porifère",
            _ => classification.ToString()
        };
    }

    public string FormatDiet(DietType diet)
    {
        return diet switch
        {
            DietType.Detritivore => "Détritivore",
            DietType.Photosynthese => "Photosynthèse",
            DietType.Heterotrophie => "Hétérotrophie",
            _ => diet.ToString()
        };
    }
    
    private static string ToSlug(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string FormatSize(double size)
    {
        return $"{size:0.##} m";
    }

    private static string FormatWeight(double weight)
    {
        return weight >= 1000
            ? $"{weight / 1000:0.##} t"
            : $"{weight:0.##} kg";
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

    public class SpeciesDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string ClassificationLabel { get; set; } = string.Empty;

        public string DietLabel { get; set; } = string.Empty;

        public string AdultSizeLabel { get; set; } = string.Empty;

        public string WeightLabel { get; set; } = string.Empty;

        public string VisibilityLabel { get; set; } = string.Empty;

        public string VisibilityCssClass { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public bool IsOwner { get; set; }
        
        public string Slug { get; set; } = string.Empty;
    }
}