using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Web.Pages.Biome;

public class SelectSpeciesModel : PageModel
{
    private const int SpeciesPerPage = 8;

    private readonly IBiomeRepository _biomeRepository;
    private readonly ISpeciesRepository _speciesRepository;
    private readonly BioDomesDbContext _context;
    private readonly ISlugService _slugService;

    public SelectSpeciesModel(
        IBiomeRepository biomeRepository,
        ISpeciesRepository speciesRepository,
        BioDomesDbContext context,
        ISlugService slugService)
    {
        _biomeRepository = biomeRepository;
        _speciesRepository = speciesRepository;
        _context = context;
        _slugService = slugService;
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

        BiomeName = biome.Name;
        BiomeSlug = _slugService.ToSlug(biome.Name);

        var linkedSpeciesIds = _context.BiomeSpeciesLinks
            .AsNoTracking()
            .Where(link => link.BiomeId == biome.Id)
            .Select(link => link.SpeciesId)
            .ToHashSet();

        var visibleSpecies = _speciesRepository.GetAll()
            .Where(species => species.IsPublicAvailable || species.Creator.Id == currentUserId);

        var filtered = ApplyFilters(visibleSpecies)
            .OrderBy(species => species.Name)
            .ToList();

        if (!IncludeAlreadyInBiome)
        {
            filtered = filtered
                .Where(species => !linkedSpeciesIds.Contains(species.Id))
                .ToList();
        }

        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)SpeciesPerPage));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        SpeciesCards = filtered
            .Skip((PageNumber - 1) * SpeciesPerPage)
            .Take(SpeciesPerPage)
            .Select(species => new SelectSpeciesCatalogItemViewModel
            {
                Id = species.Id,
                Name = species.Name,
                Slug = _slugService.ToSlug(species.Name),
                ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                    ? "/images/species/noImageSpecie.png"
                    : species.ImagePath,
                ClassificationLabel = FormatClassification(species.Classification),
                DietLabel = FormatDiet(species.Diet),
                AdultSizeLabel = $"{species.AdultSize:0.##} m",
                WeightLabel = species.Weight >= 1000
                    ? $"{species.Weight / 1000:0.##} t"
                    : $"{species.Weight:0.##} kg",
                IsPublicAvailable = species.IsPublicAvailable,
                IsAlreadyInBiome = linkedSpeciesIds.Contains(species.Id)
            })
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    public IActionResult OnPostAddToBiome(string slug, int speciesId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var species = _context.Species
            .AsNoTracking()
            .FirstOrDefault(s => s.Id == speciesId);

        if (species is null)
            return NotFound();

        if (!species.IsPublicAvailable && species.CreatorId != currentUserId)
            return Forbid();

        var existing = _context.BiomeSpeciesLinks
            .FirstOrDefault(link => link.BiomeId == biome.Id && link.SpeciesId == speciesId);

        if (existing is null)
        {
            _context.BiomeSpeciesLinks.Add(new()
            {
                BiomeId = biome.Id,
                SpeciesId = speciesId,
                IndividualCount = 1
            });
        }
        else
        {
            existing.IndividualCount += 1;
        }

        _context.SaveChanges();
        TempData["SuccessMessage"] = "Espèce ajoutée au biome.";

        return RedirectToPage("/Biome/Species", new { slug = _slugService.ToSlug(biome.Name) });
    }

    private IEnumerable<Domains.Entities.Species> ApplyFilters(IEnumerable<Domains.Entities.Species> species)
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
