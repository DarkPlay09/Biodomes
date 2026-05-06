using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// PageModel responsable de l'affichage détaillé d'une espèce.
/// La page est publique pour les espèces publiques.
/// Les espèces privées restent visibles uniquement par leur créateur ou un administrateur.
/// </summary>
[AllowAnonymous]
public class DetailsModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    private readonly ISlugService _slugService;
    private readonly ISpeciesImageStorage _speciesImageStorage;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IBiomeRepository _biomeRepository;

    public DetailsModel(
        ISpeciesRepository repository,
        ISlugService slugService,
        ISpeciesImageStorage speciesImageStorage,
        UserManager<UserEntity> userManager,
        IBiomeRepository biomeRepository)
    {
        _repository = repository;
        _slugService = slugService;
        _speciesImageStorage = speciesImageStorage;
        _userManager = userManager;
        _biomeRepository = biomeRepository;
    }

    public SpeciesDetailsViewModel SpeciesDetails { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string SafeReturnUrl
    {
        get
        {
            var fallbackUrl = Url.Page("./Index") ?? Url.Content("~/species");

            if (string.IsNullOrWhiteSpace(ReturnUrl) || !Url.IsLocalUrl(ReturnUrl))
            {
                return fallbackUrl;
            }

            var pathBase = HttpContext.Request.PathBase.Value;

            if (!string.IsNullOrWhiteSpace(pathBase)
                && ReturnUrl.StartsWith("/")
                && !ReturnUrl.StartsWith(pathBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                return pathBase + ReturnUrl;
            }

            return ReturnUrl;
        }
    }

    public async Task<IActionResult> OnGetAsync(string slug, string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            ReturnUrl = returnUrl;
        }

        var species = _repository.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        var isAuthenticated = User.Identity?.IsAuthenticated == true;

        var currentUserId = TryGetCurrentUserId(out var parsedUserId)
            ? parsedUserId
            : (int?)null;

        var isOwner = currentUserId.HasValue && species.Creator.Id == currentUserId.Value;
        var isAdmin = isAuthenticated && await IsCurrentUserAdminAsync();

        if (!species.IsPublicAvailable && !isOwner && !isAdmin)
        {
            return isAuthenticated
                ? Forbid()
                : NotFound();
        }

        var canManage = CanManageSpecies(isOwner, isAdmin, species.IsPublicAvailable);

        var impactedBiomesCount = canManage
            ? _biomeRepository.CountBiomesUsingSpecies(species.Id)
            : 0;

        SpeciesDetails = MapToDetails(
            species,
            isOwner,
            canManage,
            impactedBiomesCount);

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string slug, string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            ReturnUrl = returnUrl;
        }

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
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!CanManageSpecies(isOwner, isAdmin, species.IsPublicAvailable))
        {
            return Forbid();
        }

        var impactedBiomesCount = _biomeRepository.CountBiomesUsingSpecies(species.Id);

        if (impactedBiomesCount > 0)
        {
            TempData["ErrorMessage"] =
                $"Suppression impossible : l'espèce « {species.Name} » est utilisée dans {impactedBiomesCount} biome(s).";

            return RedirectToPage(new
            {
                slug,
                returnUrl = ReturnUrl
            });
        }

        _repository.DeleteBySlug(slug);
        _speciesImageStorage.Delete(species.ImagePath);

        TempData["SuccessMessage"] = $"L'espèce « {species.Name} » a bien été supprimée.";

        return LocalRedirect(SafeReturnUrl);
    }

    private async Task<bool> IsCurrentUserAdminAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var user = await _userManager.GetUserAsync(User);

        return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private SpeciesDetailsViewModel MapToDetails(
        SpeciesEntity species,
        bool isOwner,
        bool canManage,
        int impactedBiomesCount)
    {
        return new SpeciesDetailsViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = _slugService.ToSlug(species.Name),
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
            IsOwner = isOwner,
            CanManage = canManage,
            ImpactedBiomesCount = impactedBiomesCount
        };
    }

    private static bool CanManageSpecies(bool isOwner, bool isAdmin, bool isPublicAvailable)
    {
        return (isOwner && !isPublicAvailable)
               || (isAdmin && isPublicAvailable);
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

        public string Slug { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string ClassificationLabel { get; set; } = string.Empty;

        public string DietLabel { get; set; } = string.Empty;

        public string AdultSizeLabel { get; set; } = string.Empty;

        public string WeightLabel { get; set; } = string.Empty;

        public string VisibilityLabel { get; set; } = string.Empty;

        public string VisibilityCssClass { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public bool IsOwner { get; set; }

        public bool CanManage { get; set; }

        public int ImpactedBiomesCount { get; set; }
    }
}