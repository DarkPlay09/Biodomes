using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Admin.Species;

/// <summary>
/// Page d'administration permettant de gérer la visibilité des espèces créées par les utilisateurs.
/// </summary>
public class IndexModel : PageModel
{
    private const int SpeciesPerPage = 8;

    private readonly ISpeciesRepository _speciesRepository;
    private readonly ISlugService _slugService;
    private readonly UserManager<UserEntity> _userManager;

    public IndexModel(
        ISpeciesRepository speciesRepository,
        ISlugService slugService,
        UserManager<UserEntity> userManager)
    {
        _speciesRepository = speciesRepository;
        _slugService = slugService;
        _userManager = userManager;
    }

    /// <summary>
    /// Message temporaire affiché dans le toast après une action.
    /// </summary>
    [TempData]
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public SpeciesClassification? ClassificationFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DietType? DietFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? VisibilityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int TotalPages { get; private set; }

    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    public IReadOnlyList<AdminSpeciesViewModel> SpeciesList { get; private set; } = [];

    public IReadOnlyList<SpeciesClassification> ClassificationOptions { get; } =
        Enum.GetValues<SpeciesClassification>();

    public IReadOnlyList<DietType> DietOptions { get; } =
        Enum.GetValues<DietType>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetCurrentUserId(out _))
        {
            return Challenge();
        }

        if (!await IsCurrentUserAdminAsync())
        {
            return Forbid();
        }

        var allSpecies = _speciesRepository.GetAll().ToList();

        var filteredSpecies = ApplyFilters(allSpecies)
            .OrderBy(species => species.IsPublicAvailable)
            .ThenBy(species => species.Name)
            .ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredSpecies.Count / (double)SpeciesPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        SpeciesList = filteredSpecies
            .Skip((PageNumber - 1) * SpeciesPerPage)
            .Take(SpeciesPerPage)
            .Select(MapToViewModel)
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    public async Task<IActionResult> OnPostSetVisibilityAsync(string slug, bool makePublic)
    {
        if (!TryGetCurrentUserId(out _))
        {
            return Challenge();
        }

        if (!await IsCurrentUserAdminAsync())
        {
            return Forbid();
        }

        var species = _speciesRepository.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        species.IsPublicAvailable = makePublic;

        _speciesRepository.Update(slug, species);

        SuccessMessage = makePublic
            ? $"L'espèce « {species.Name} » est maintenant disponible pour tous les utilisateurs."
            : $"L'espèce « {species.Name} » est maintenant non visible pour les autres utilisateurs.";

        return RedirectToPage(new
        {
            Search,
            ClassificationFilter,
            DietFilter,
            VisibilityFilter,
            PageNumber
        });
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

    private IEnumerable<SpeciesEntity> ApplyFilters(IEnumerable<SpeciesEntity> species)
    {
        var filteredSpecies = species;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatClassification(currentSpecies.Classification).Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatDiet(currentSpecies.Diet).Contains(search, StringComparison.OrdinalIgnoreCase)
                || currentSpecies.Creator.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (ClassificationFilter.HasValue)
        {
            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Classification == ClassificationFilter.Value);
        }

        if (DietFilter.HasValue)
        {
            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Diet == DietFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(VisibilityFilter))
        {
            var visibility = VisibilityFilter.Trim().ToLowerInvariant();

            filteredSpecies = visibility switch
            {
                "public" => filteredSpecies.Where(currentSpecies => currentSpecies.IsPublicAvailable),
                "private" => filteredSpecies.Where(currentSpecies => !currentSpecies.IsPublicAvailable),
                _ => filteredSpecies
            };
        }

        return filteredSpecies;
    }

    private AdminSpeciesViewModel MapToViewModel(SpeciesEntity species)
    {
        return new AdminSpeciesViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = _slugService.ToSlug(species.Name),
            ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                ? "/uploads/species/noImageSpecie.png"
                : species.ImagePath,
            ClassificationLabel = FormatClassification(species.Classification),
            DietLabel = FormatDiet(species.Diet),
            CreatorName = string.IsNullOrWhiteSpace(species.Creator.UserName)
                ? "Utilisateur inconnu"
                : species.Creator.UserName,
            IsPublicAvailable = species.IsPublicAvailable,
            VisibilityLabel = species.IsPublicAvailable ? "Disponible" : "Non visible"
        };
    }

    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

    private static IReadOnlyList<int> BuildVisiblePages(int currentPage, int totalPages)
    {
        if (totalPages <= 3)
        {
            return Enumerable.Range(1, totalPages).ToList();
        }

        var start = Math.Max(1, currentPage - 1);
        var end = Math.Min(totalPages, start + 2);

        start = Math.Max(1, end - 2);

        return Enumerable.Range(start, end - start + 1).ToList();
    }

    public class AdminSpeciesViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string ClassificationLabel { get; set; } = string.Empty;

        public string DietLabel { get; set; } = string.Empty;

        public string CreatorName { get; set; } = string.Empty;

        public bool IsPublicAvailable { get; set; }

        public string VisibilityLabel { get; set; } = string.Empty;
    }
}
