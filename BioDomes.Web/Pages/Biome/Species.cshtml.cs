using System.Security.Claims;
using BioDomes.Domains.Queries.Biome.Species;
using BioDomes.Domains.Repositories;
using BioDomes.Domains.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class SpeciesModel : PageModel
{
    private readonly IBiomeRepository _biomeRepository;
    private readonly IBiomeStabilityService _biomeStabilityService;

    public SpeciesModel(IBiomeRepository biomeRepository, IBiomeStabilityService biomeStabilityService)
    {
        _biomeRepository = biomeRepository;
        _biomeStabilityService = biomeStabilityService;
    }

    public BiomeSpeciesPageViewModel? PageData { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ClassificationFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DietFilter { get; set; }

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        BuildPageData(slug, currentUserId);

        return Page();
    }

    public IActionResult OnPostAdjust(string slug, int speciesId, int delta)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var managementData = _biomeRepository.GetSpeciesManagementPageData(
            slug,
            currentUserId,
            new BiomeSpeciesManagementFiltersDto());

        if (managementData is null)
            return NotFound();

        var targetSpecies = managementData.SpeciesCards
            .FirstOrDefault(species => species.SpeciesId == speciesId);

        if (targetSpecies is null)
            return NotFound();

        var newCount = Math.Max(0, targetSpecies.CurrentIndividualCount + delta);
        _biomeRepository.SetSpeciesCountInBiome(biome.Id, speciesId, newCount);

        return RedirectToPage(new
        {
            slug,
            Search,
            ClassificationFilter,
            DietFilter
        });
    }

    public IActionResult OnPostSetCount(string slug, int speciesId, int count)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var safeCount = Math.Max(0, count);
        _biomeRepository.SetSpeciesCountInBiome(biome.Id, speciesId, safeCount);

        TempData["SuccessMessage"] = "Population mise à jour.";

        return RedirectToPage(new
        {
            slug,
            Search,
            ClassificationFilter,
            DietFilter
        });
    }

    public IActionResult OnPostRemove(string slug, int speciesId)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        _biomeRepository.SetSpeciesCountInBiome(biome.Id, speciesId, 0);

        return RedirectToPage(new
        {
            slug,
            Search,
            ClassificationFilter,
            DietFilter
        });
    }

    private void BuildPageData(string biomeSlug, int currentUserId)
    {
        var filters = new BiomeSpeciesManagementFiltersDto
        {
            Search = Search,
            Classification = ClassificationFilter,
            Diet = DietFilter
        };

        var managementData = _biomeRepository.GetSpeciesManagementPageData(
            biomeSlug,
            currentUserId,
            filters);

        if (managementData is null)
        {
            PageData = null;
            return;
        }

        var detailsData = _biomeRepository.GetDetailsBySlugForCreator(biomeSlug, currentUserId);

        var linkedSpecies = managementData.SpeciesCards
            .Select(species => new BiomeLinkedSpeciesViewModel(
                species.SpeciesId,
                species.Name,
                species.Slug,
                string.IsNullOrWhiteSpace(species.ImagePath)
                    ? "/uploads/species/noImageSpecie.png"
                    : species.ImagePath,
                species.Classification,
                species.Diet,
                species.CurrentIndividualCount,
                species.IsPublicAvailable))
            .ToList();

        var classifications = linkedSpecies
            .Select(species => species.Classification)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();

        var diets = linkedSpecies
            .Select(species => species.Diet)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();

        var speciesAlerts = detailsData is null
            ? Array.Empty<string>()
            : BiomeAlertsBuilder.BuildSpeciesAlerts(detailsData.Species, _biomeStabilityService);

        PageData = new BiomeSpeciesPageViewModel(
            BiomeId: managementData.BiomeId,
            BiomeName: managementData.BiomeName,
            BiomeSlug: managementData.BiomeSlug,
            LinkedSpecies: linkedSpecies,
            ClassificationOptions: classifications,
            DietOptions: diets,
            SpeciesAlerts: speciesAlerts);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId) && userId > 0;
    }
}

public sealed record BiomeSpeciesPageViewModel(
    int BiomeId,
    string BiomeName,
    string BiomeSlug,
    IReadOnlyList<BiomeLinkedSpeciesViewModel> LinkedSpecies,
    IReadOnlyList<string> ClassificationOptions,
    IReadOnlyList<string> DietOptions,
    IReadOnlyList<string> SpeciesAlerts);

public sealed record BiomeLinkedSpeciesViewModel(
    int SpeciesId,
    string Name,
    string Slug,
    string ImagePath,
    string Classification,
    string Diet,
    int IndividualCount,
    bool IsPublicAvailable);
