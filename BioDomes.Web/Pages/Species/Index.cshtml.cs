using System.Globalization;
using System.Security.Claims;
using System.Text;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Species;

public class SpeciesModel : PageModel
{
    private const int SpeciesPerPage = 12;

    private readonly ISpeciesRepository _repository;
    private readonly IWebHostEnvironment _environment;

    public SpeciesModel(
        ISpeciesRepository repository,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }

    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

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

    public int PageSize => SpeciesPerPage;

    public int TotalSpeciesCount { get; private set; }

    public int FilteredSpeciesCount { get; private set; }

    public int TotalPages { get; private set; }

    public int FirstItemNumber { get; private set; }

    public int LastItemNumber { get; private set; }

    public IReadOnlyList<SpeciesCatalogItemViewModel> SpeciesCards { get; private set; } = [];

    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    public IReadOnlyList<SpeciesClassification> ClassificationOptions { get; } =
        Enum.GetValues<SpeciesClassification>();

    public IReadOnlyList<DietType> DietOptions { get; } =
        Enum.GetValues<DietType>();

    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var visibleSpecies = _repository.GetAll()
            .Where(species => species.IsPublicAvailable || species.Creator.Id == currentUserId)
            .ToList();

        TotalSpeciesCount = visibleSpecies.Count;

        var filteredSpecies = ApplyFilters(visibleSpecies)
            .OrderBy(species => species.Name)
            .ToList();

        FilteredSpeciesCount = filteredSpecies.Count;

        TotalPages = Math.Max(1, (int)Math.Ceiling(FilteredSpeciesCount / (double)SpeciesPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        FirstItemNumber = FilteredSpeciesCount == 0
            ? 0
            : ((PageNumber - 1) * SpeciesPerPage) + 1;

        LastItemNumber = Math.Min(PageNumber * SpeciesPerPage, FilteredSpeciesCount);

        SpeciesCards = filteredSpecies
            .Skip((PageNumber - 1) * SpeciesPerPage)
            .Take(SpeciesPerPage)
            .Select(species => MapToCatalogItem(species, currentUserId))
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    public IActionResult OnPostDelete(string slug)
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

        if (species.Creator.Id != currentUserId)
        {
            return Forbid();
        }

        _repository.DeleteBySlug(slug);

        if (!string.IsNullOrWhiteSpace(species.ImagePath)
            && species.ImagePath.StartsWith("/images/species/")
            && species.ImagePath != "/images/species/noImageSpecie.png")
        {
            var relativePath = species.ImagePath
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        return RedirectToPage();
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
                || FormatDiet(currentSpecies.Diet).Contains(search, StringComparison.OrdinalIgnoreCase));
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

    private SpeciesCatalogItemViewModel MapToCatalogItem(SpeciesEntity species, int currentUserId)
    {
        return new SpeciesCatalogItemViewModel
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
            IsPublicAvailable = species.IsPublicAvailable,
            CanEdit = species.Creator.Id == currentUserId
        };
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

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && userId > 0;
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

    public class SpeciesCatalogItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string ClassificationLabel { get; set; } = string.Empty;

        public string DietLabel { get; set; } = string.Empty;

        public string AdultSizeLabel { get; set; } = string.Empty;

        public string WeightLabel { get; set; } = string.Empty;

        public bool IsPublicAvailable { get; set; }

        public bool CanEdit { get; set; }
    }
}