using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Queries.Biome.SelectEquipment;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class SelectEquipmentModel : PageModel
{
    private const int EquipmentsPerPage = 8;

    private readonly IBiomeRepository _biomeRepository;

    public SelectEquipmentModel(IBiomeRepository biomeRepository)
    {
        _biomeRepository = biomeRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public ResourceType? ProducedElementFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public ResourceType? ConsumedElementFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? VisibilityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public bool IncludeAlreadyInBiome { get; set; }

    // Conservé pour réutiliser select-species-selection.js sans modification.
    [BindProperty]
    public List<int> SelectedSpeciesIds { get; set; } = [];

    public int TotalPages { get; private set; }
    public string BiomeName { get; private set; } = string.Empty;
    public string BiomeSlug { get; private set; } = string.Empty;

    public IReadOnlyList<SelectEquipmentCatalogItemViewModel> EquipmentCards { get; private set; } = [];
    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    public IReadOnlyList<ResourceType> ResourceOptions { get; } = Enum.GetValues<ResourceType>();

    public IActionResult OnGet(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var biome = _biomeRepository.GetBySlug(slug);
        if (biome is null)
            return NotFound();

        if (biome.Creator.Id != currentUserId)
            return Forbid();

        var pageData = _biomeRepository.GetSelectEquipmentPageData(slug, currentUserId);
        if (pageData is null)
            return NotFound();

        BiomeName = pageData.BiomeName;
        BiomeSlug = pageData.BiomeSlug;

        var filtered = ApplyFilters(pageData.Equipments)
            .OrderBy(equipment => equipment.Name)
            .ToList();

        if (!IncludeAlreadyInBiome)
        {
            filtered = filtered
                .Where(equipment => !equipment.IsAlreadyInBiome)
                .ToList();
        }

        TotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)EquipmentsPerPage));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        EquipmentCards = filtered
            .Skip((PageNumber - 1) * EquipmentsPerPage)
            .Take(EquipmentsPerPage)
            .Select(equipment => new SelectEquipmentCatalogItemViewModel
            {
                Id = equipment.EquipmentId,
                Name = equipment.Name,
                Slug = equipment.Slug,
                ImagePath = NormalizeWebPath(equipment.ImagePath),
                ProducedElementLabel = FormatNullableResource(equipment.ProducedElement),
                ConsumedElementLabel = FormatNullableResource(equipment.ConsumedElement),
                IsPublicAvailable = equipment.IsPublicAvailable,
                IsAlreadyInBiome = equipment.IsAlreadyInBiome
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
            TempData["SuccessMessage"] = "Aucun équipement sélectionné.";
            return RedirectToPage(new { slug });
        }

        _biomeRepository.AddEquipmentToBiome(biome.Id, selectedIds);
        TempData["SuccessMessage"] = $"{selectedIds.Count} équipement(s) ajouté(s) au biome.";

        return RedirectToPage("/Biome/Details", new { slug });
    }

    private IEnumerable<SelectEquipmentCardDto> ApplyFilters(IEnumerable<SelectEquipmentCardDto> equipments)
    {
        var filtered = equipments;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            filtered = filtered.Where(currentEquipment =>
                currentEquipment.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ProducedElement).Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ConsumedElement).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (ProducedElementFilter.HasValue)
        {
            var produced = ProducedElementFilter.Value.ToString();
            filtered = filtered.Where(currentEquipment =>
                string.Equals(currentEquipment.ProducedElement, produced, StringComparison.OrdinalIgnoreCase));
        }

        if (ConsumedElementFilter.HasValue)
        {
            var consumed = ConsumedElementFilter.Value.ToString();
            filtered = filtered.Where(currentEquipment =>
                string.Equals(currentEquipment.ConsumedElement, consumed, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(VisibilityFilter))
        {
            var visibility = VisibilityFilter.Trim().ToLowerInvariant();
            filtered = visibility switch
            {
                "public" => filtered.Where(currentEquipment => currentEquipment.IsPublicAvailable),
                "private" => filtered.Where(currentEquipment => !currentEquipment.IsPublicAvailable),
                _ => filtered
            };
        }

        return filtered;
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

    private static string NormalizeWebPath(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return "/uploads/equipment/noImageEquipment.png";

        var normalized = imagePath.Trim().Replace("\\", "/");

        if (normalized.StartsWith("~/", StringComparison.Ordinal))
            normalized = normalized[1..];

        if (!normalized.StartsWith("/", StringComparison.Ordinal))
            normalized = "/" + normalized;

        return normalized;
    }

    private static string FormatNullableResource(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Aucun";

        return value switch
        {
            "Lumiere" => "Lumière",
            _ => value
        };
    }
}

public class SelectEquipmentCatalogItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ImagePath { get; init; } = string.Empty;
    public string ProducedElementLabel { get; init; } = string.Empty;
    public string ConsumedElementLabel { get; init; } = string.Empty;
    public bool IsPublicAvailable { get; init; }
    public bool IsAlreadyInBiome { get; init; }
}
