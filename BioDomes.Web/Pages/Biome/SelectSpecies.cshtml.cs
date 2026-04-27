using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Queries.Species;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class SelectSpeciesModel : PageModel
{
    private const int SpeciesPerPage = 8;

    private readonly IBiomeRepository _biomeRepository;

    public SelectSpeciesModel(IBiomeRepository biomeRepository)
    {
        _biomeRepository = biomeRepository;
    }

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

    [BindProperty(SupportsGet = true)]
    public bool IncludeAlreadyInBiome { get; set; }

    [BindProperty]
    public List<int> SelectedSpeciesIds { get; set; } = [];

    public int TotalPages { get; private set; }
    public string BiomeName { get; private set; } = string.Empty;
    public string BiomeSlug { get; private set; } = string.Empty;

    public IReadOnlyList<SelectSpeciesCatalogItemViewModel> SpeciesCards { get; private set; } = [];
    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    public IReadOnlyList<SpeciesClassification> ClassificationOptions { get; } =
        Enum.GetValues<SpeciesClassification>();

    public IReadOnlyList<DietType> DietOptions { get; } =
        Enum.GetValues<DietType>();

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var pageData = _biomeRepository.GetSelectSpeciesPageData(slug, currentUserId);
        if (pageData is null)
            return NotFound();

        BiomeName = pageData.BiomeName;
        BiomeSlug = pageData.BiomeSlug;

        var filtered = ApplyFilters(pageData.Species)
            .OrderBy(species => species.Name)
            .ToList();

        if (!IncludeAlreadyInBiome)
        {
            filtered = filtered
                .Where(species => !species.IsAlreadyInBiome)
                .ToList();
        }

        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)SpeciesPerPage));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        SpeciesCards = filtered
            .Skip((PageNumber - 1) * SpeciesPerPage)
            .Take(SpeciesPerPage)
            .Select(species => new SelectSpeciesCatalogItemViewModel
            {
                Id = species.SpeciesId,
                Name = species.Name,
                Slug = species.Slug,
                ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                    ? "/images/species/noImageSpecie.png"
                    : species.ImagePath,
                ClassificationLabel = species.Classification,
                DietLabel = species.Diet,
                AdultSizeLabel = $"{species.AdultSize:0.##} m",
                WeightLabel = species.Weight >= 1000
                    ? $"{species.Weight / 1000:0.##} t"
                    : $"{species.Weight:0.##} kg",
                IsPublicAvailable = species.IsPublicAvailable,
                IsAlreadyInBiome = species.IsAlreadyInBiome
            })
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    public IActionResult OnPostConfirmSelection(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var selectedIds = SelectedSpeciesIds
            .Distinct()
            .ToList();

        if (selectedIds.Count == 0)
        {
            TempData["SuccessMessage"] = "Aucune espèce sélectionnée.";
            return RedirectToPage(new { slug });
        }

        _biomeRepository.AddSpeciesToBiome(biome.Id, selectedIds, 1);
        TempData["SuccessMessage"] = $"{selectedIds.Count} espèce(s) ajoutée(s) au biome.";

        return RedirectToPage("/Biome/Species", new { slug });
    }

    private IEnumerable<SelectSpeciesCardDto> ApplyFilters(IEnumerable<SelectSpeciesCardDto> species)
    {
        var filteredSpecies = species;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || currentSpecies.Classification.Contains(search, StringComparison.OrdinalIgnoreCase)
                || currentSpecies.Diet.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (ClassificationFilter.HasValue)
        {
            var expectedEnumName = ClassificationFilter.Value.ToString();
            var expectedLabel = FormatClassification(ClassificationFilter.Value);

            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                string.Equals(currentSpecies.Classification, expectedEnumName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(currentSpecies.Classification, expectedLabel, StringComparison.OrdinalIgnoreCase));
        }

        if (DietFilter.HasValue)
        {
            var expectedEnumName = DietFilter.Value.ToString();
            var expectedLabel = FormatDiet(DietFilter.Value);

            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                string.Equals(currentSpecies.Diet, expectedEnumName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(currentSpecies.Diet, expectedLabel, StringComparison.OrdinalIgnoreCase));
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

    private static IReadOnlyList<int> BuildVisiblePages(int currentPage, int totalPages)
    {
        if (totalPages <= 3)
            return Enumerable.Range(1, totalPages).ToList();

        var start = Math.Max(1, currentPage - 1);
        var end = Math.Min(totalPages, start + 2);
        start = Math.Max(1, end - 2);

        return Enumerable.Range(start, end - start + 1).ToList();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId) && userId > 0;
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
}

public class SelectSpeciesCatalogItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public string ClassificationLabel { get; init; } = string.Empty;
    public string DietLabel { get; init; } = string.Empty;
    public string AdultSizeLabel { get; init; } = string.Empty;
    public string WeightLabel { get; init; } = string.Empty;
    public bool IsPublicAvailable { get; init; }
    public bool IsAlreadyInBiome { get; init; }
}
