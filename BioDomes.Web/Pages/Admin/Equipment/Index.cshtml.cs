using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using EquipmentEntity = BioDomes.Domains.Entities.Equipment;

namespace BioDomes.Web.Pages.Admin.Equipment;

/// <summary>
/// Page d'administration permettant de gérer la visibilité des équipements créés par les utilisateurs.
/// </summary>
public class IndexModel : PageModel
{
    private const int EquipmentPerPage = 8;

    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ISlugService _slugService;
    private readonly UserManager<UserEntity> _userManager;

    public IndexModel(
        IEquipmentRepository equipmentRepository,
        ISlugService slugService,
        UserManager<UserEntity> userManager)
    {
        _equipmentRepository = equipmentRepository;
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
    public ResourceType? ProducedElementFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public ResourceType? ConsumedElementFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? VisibilityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int TotalPages { get; private set; }

    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    public IReadOnlyList<AdminEquipmentViewModel> EquipmentList { get; private set; } = [];

    public IReadOnlyList<ResourceType> ResourceOptions { get; } =
        Enum.GetValues<ResourceType>();

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

        var equipmentCreatedByUsers = _equipmentRepository.GetAll()
            .Where(equipment => !IsCreatedByAdmin(equipment))
            .ToList();

        var filteredEquipment = ApplyFilters(equipmentCreatedByUsers)
            .OrderBy(equipment => equipment.IsPublicAvailable)
            .ThenBy(equipment => equipment.Name)
            .ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredEquipment.Count / (double)EquipmentPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        EquipmentList = filteredEquipment
            .Skip((PageNumber - 1) * EquipmentPerPage)
            .Take(EquipmentPerPage)
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

        var equipment = _equipmentRepository.GetBySlug(slug);

        if (equipment is null)
        {
            return NotFound();
        }

        if (IsCreatedByAdmin(equipment))
        {
            return Forbid();
        }

        equipment.IsPublicAvailable = makePublic;

        _equipmentRepository.Update(slug, equipment);

        SuccessMessage = makePublic
            ? $"L'équipement « {equipment.Name} » est maintenant disponible pour tous les utilisateurs."
            : $"L'équipement « {equipment.Name} » est maintenant non visible pour les autres utilisateurs.";

        return RedirectToPage(new
        {
            Search,
            ProducedElementFilter,
            ConsumedElementFilter,
            VisibilityFilter,
            PageNumber
        });
    }

    public string FormatResource(ResourceType resource)
    {
        return resource switch
        {
            ResourceType.Eau => "Eau",
            ResourceType.Lumiere => "Lumière",
            ResourceType.Azote => "Azote",
            ResourceType.Hydrogene => "Hydrogène",
            _ => resource.ToString()
        };
    }

    private IEnumerable<EquipmentEntity> ApplyFilters(IEnumerable<EquipmentEntity> equipment)
    {
        var filteredEquipment = equipment;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            filteredEquipment = filteredEquipment.Where(currentEquipment =>
                currentEquipment.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ProducedElement).Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ConsumedElement).Contains(search, StringComparison.OrdinalIgnoreCase)
                || currentEquipment.Creator.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (ProducedElementFilter.HasValue)
        {
            filteredEquipment = filteredEquipment.Where(currentEquipment =>
                currentEquipment.ProducedElement == ProducedElementFilter.Value);
        }

        if (ConsumedElementFilter.HasValue)
        {
            filteredEquipment = filteredEquipment.Where(currentEquipment =>
                currentEquipment.ConsumedElement == ConsumedElementFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(VisibilityFilter))
        {
            var visibility = VisibilityFilter.Trim().ToLowerInvariant();

            filteredEquipment = visibility switch
            {
                "public" => filteredEquipment.Where(currentEquipment => currentEquipment.IsPublicAvailable),
                "private" => filteredEquipment.Where(currentEquipment => !currentEquipment.IsPublicAvailable),
                _ => filteredEquipment
            };
        }

        return filteredEquipment;
    }

    private AdminEquipmentViewModel MapToViewModel(EquipmentEntity equipment)
    {
        return new AdminEquipmentViewModel
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Slug = _slugService.ToSlug(equipment.Name),
            ImagePath = string.IsNullOrWhiteSpace(equipment.ImagePath)
                ? "/images/equipment/noImageEquipment.png"
                : equipment.ImagePath,
            ProducedElementLabel = FormatNullableResource(equipment.ProducedElement),
            ConsumedElementLabel = FormatNullableResource(equipment.ConsumedElement),
            CreatorName = string.IsNullOrWhiteSpace(equipment.Creator.UserName)
                ? "Utilisateur inconnu"
                : equipment.Creator.UserName,
            IsPublicAvailable = equipment.IsPublicAvailable,
            VisibilityLabel = equipment.IsPublicAvailable ? "Disponible" : "Non visible"
        };
    }

    private static bool IsCreatedByAdmin(EquipmentEntity equipment)
    {
        return equipment.Creator.IsAdmin;
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

    private string FormatNullableResource(ResourceType? resource)
    {
        return resource.HasValue
            ? FormatResource(resource.Value)
            : "-";
    }

    public class AdminEquipmentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string ProducedElementLabel { get; set; } = string.Empty;

        public string ConsumedElementLabel { get; set; } = string.Empty;

        public string CreatorName { get; set; } = string.Empty;

        public bool IsPublicAvailable { get; set; }

        public string VisibilityLabel { get; set; } = string.Empty;
    }
}