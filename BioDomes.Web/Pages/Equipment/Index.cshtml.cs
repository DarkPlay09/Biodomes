using System.Security.Claims;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using EquipmentEntity = BioDomes.Domains.Entities.Equipment;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// PageModel du catalogue des équipements.
/// Elle gère l'affichage des équipements visibles par l'utilisateur connecté,
/// les filtres, la recherche, la pagination et la suppression.
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// Nombre maximal d'équipements affichés par page.
    /// </summary>
    private const int EquipmentPerPage = 8;

    private readonly IEquipmentRepository _repository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;
    private readonly ISlugService _slugService;
    private readonly UserManager<UserEntity> _userManager;

    /// <summary>
    /// Initialise la page catalogue des équipements.
    /// </summary>
    /// <param name="repository">Repository permettant de lire et supprimer les équipements.</param>
    /// <param name="equipmentImageStorage">Service permettant de supprimer l'image liée à un équipement.</param>
    /// <param name="slugService">Service slug pour les liens des équipements.</param>
    /// <param name="userManager">...</param>
    public IndexModel(
        IEquipmentRepository repository,
        IEquipmentImageStorage equipmentImageStorage,
        ISlugService slugService,
        UserManager<UserEntity> userManager)
    {
        _repository = repository;
        _equipmentImageStorage = equipmentImageStorage;
        _slugService = slugService;
        _userManager = userManager;
    }

    /// <summary>
    /// Texte recherché dans le nom, la ressource produite ou la ressource consommée.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    /// <summary>
    /// Filtre optionnel sur la ressource produite par l'équipement.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public ResourceType? ProducedElementFilter { get; set; }

    /// <summary>
    /// Filtre optionnel sur la ressource consommée par l'équipement.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public ResourceType? ConsumedElementFilter { get; set; }

    /// <summary>
    /// Filtre optionnel sur la visibilité.
    /// Les valeurs attendues sont "public", "private" ou vide.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? VisibilityFilter { get; set; }

    /// <summary>
    /// Numéro de page demandé dans la pagination.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Nombre d'équipements affichés par page.
    /// </summary>
    public int PageSize => EquipmentPerPage;

    /// <summary>
    /// Nombre total d'équipements visibles avant filtrage.
    /// </summary>
    public int TotalEquipmentCount { get; private set; }

    /// <summary>
    /// Nombre total d'équipements après application des filtres.
    /// </summary>
    public int FilteredEquipmentCount { get; private set; }

    /// <summary>
    /// Nombre total de pages après filtrage.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Numéro du premier élément affiché sur la page courante.
    /// </summary>
    public int FirstItemNumber { get; private set; }

    /// <summary>
    /// Numéro du dernier élément affiché sur la page courante.
    /// </summary>
    public int LastItemNumber { get; private set; }

    /// <summary>
    /// Liste des équipements convertis en cartes pour l'affichage.
    /// </summary>
    public IReadOnlyList<EquipmentCatalogItemViewModel> EquipmentCards { get; private set; } = [];

    /// <summary>
    /// Numéros de pages affichés dans la pagination.
    /// </summary>
    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    /// <summary>
    /// Options disponibles pour le filtre de ressource.
    /// </summary>
    public IReadOnlyList<ResourceType> ResourceOptions { get; } =
        Enum.GetValues<ResourceType>();

    /// <summary>
    /// Message temporaire affiché lorsqu'un equipement vient d'être ajouté.
    /// </summary>
    [TempData]
    public string? LastInsertedEquipmentName { get; set; }

    /// <summary>
    /// Charge les équipements visibles, applique les filtres,
    /// calcule la pagination et prépare les cartes du catalogue.
    /// </summary>
    /// <returns>La page catalogue ou une demande de connexion.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var isAdmin = await IsCurrentUserAdminAsync();

        var visibleEquipment = _repository.GetAll()
            .Where(equipment => equipment.IsPublicAvailable || equipment.Creator.Id == currentUserId)
            .ToList();

        TotalEquipmentCount = visibleEquipment.Count;

        var filteredEquipment = ApplyFilters(visibleEquipment)
            .OrderBy(equipment => equipment.Name)
            .ToList();

        FilteredEquipmentCount = filteredEquipment.Count;

        TotalPages = Math.Max(1, (int)Math.Ceiling(FilteredEquipmentCount / (double)EquipmentPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        FirstItemNumber = FilteredEquipmentCount == 0
            ? 0
            : ((PageNumber - 1) * EquipmentPerPage) + 1;

        LastItemNumber = Math.Min(PageNumber * EquipmentPerPage, FilteredEquipmentCount);

        EquipmentCards = filteredEquipment
            .Skip((PageNumber - 1) * EquipmentPerPage)
            .Take(EquipmentPerPage)
            .Select(equipment => MapToCatalogItem(equipment, currentUserId, isAdmin))
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    /// <summary>
    /// Supprime un équipement appartenant à l'utilisateur connecté.
    /// L'image associée est également supprimée du stockage.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à supprimer.</param>
    /// <returns>Une redirection vers la page courante ou une erreur si l'action est impossible.</returns>
    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var equipment = _repository.GetBySlug(slug);

        if (equipment is null)
        {
            return NotFound();
        }

        if (equipment.Creator.Id != currentUserId)
        {
            return Forbid();
        }

        _repository.DeleteBySlug(slug);
        _equipmentImageStorage.Delete(equipment.ImagePath);

        return RedirectToPage();
    }

    /// <summary>
    /// Formate une ressource du domaine en libellé français lisible.
    /// </summary>
    /// <param name="resource">Ressource à formater.</param>
    /// <returns>Libellé affichable.</returns>
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

    /// <summary>
    /// Applique la recherche et les filtres sélectionnés par l'utilisateur.
    /// </summary>
    /// <param name="equipment">Liste d'équipements visibles avant filtrage.</param>
    /// <returns>Liste filtrée.</returns>
    private IEnumerable<EquipmentEntity> ApplyFilters(IEnumerable<EquipmentEntity> equipment)
    {
        var filteredEquipment = equipment;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            filteredEquipment = filteredEquipment.Where(currentEquipment =>
                currentEquipment.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ProducedElement).Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatNullableResource(currentEquipment.ConsumedElement).Contains(search, StringComparison.OrdinalIgnoreCase));
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

    /// <summary>
    /// Convertit un équipement du domaine en ViewModel de carte catalogue.
    /// </summary>
    /// <param name="equipment">Équipement à afficher.</param>
    /// <param name="currentUserId">Identifiant de l'utilisateur connecté.</param>
    /// <returns>Carte prête à être affichée.</returns>
    private EquipmentCatalogItemViewModel MapToCatalogItem(
        EquipmentEntity equipment,
        int currentUserId,
        bool isAdmin)
    {
        var isOwner = equipment.Creator.Id == currentUserId;

        return new EquipmentCatalogItemViewModel
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Slug = _slugService.ToSlug(equipment.Name),
            ImagePath = string.IsNullOrWhiteSpace(equipment.ImagePath)
                ? "/images/equipment/noImageEquipment.png"
                : equipment.ImagePath,
            ProducedElementLabel = FormatNullableResource(equipment.ProducedElement),
            ConsumedElementLabel = FormatNullableResource(equipment.ConsumedElement),
            IsPublicAvailable = equipment.IsPublicAvailable,
            CanEdit = CanManageEquipment(isOwner, isAdmin, equipment.IsPublicAvailable)
        };
    }
    
    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanManageEquipment(bool isOwner, bool isAdmin, bool isPublicAvailable)
    {
        return (isOwner && !isPublicAvailable)
               || (isAdmin && isPublicAvailable);
    }

    /// <summary>
    /// Calcule les numéros de pages visibles dans la pagination.
    /// La pagination affiche maximum trois pages à la fois.
    /// </summary>
    /// <param name="currentPage">Page actuellement affichée.</param>
    /// <param name="totalPages">Nombre total de pages.</param>
    /// <returns>Liste des pages à afficher.</returns>
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

    /// <summary>
    /// Récupère l'identifiant de l'utilisateur connecté depuis les claims Identity.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur connecté si la récupération réussit.</param>
    /// <returns>True si l'identifiant est valide, sinon false.</returns>
    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

    /// <summary>
    /// Formate une ressource optionnelle.
    /// </summary>
    /// <param name="resource">Ressource à formater, ou null.</param>
    /// <returns>Libellé affichable.</returns>
    private string FormatNullableResource(ResourceType? resource)
    {
        return resource.HasValue
            ? FormatResource(resource.Value)
            : "-";
    }

    /// <summary>
    /// ViewModel représentant un équipement affiché dans le catalogue.
    /// </summary>
    public class EquipmentCatalogItemViewModel
    {
        /// <summary>
        /// Identifiant technique de l'équipement.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nom de l'équipement.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slug utilisé pour générer les liens vers les pages détails ou modification.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Chemin de l'image affichée sur la carte.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Ressource produite par l'équipement.
        /// </summary>
        public string ProducedElementLabel { get; set; } = string.Empty;

        /// <summary>
        /// Ressource consommée par l'équipement.
        /// </summary>
        public string ConsumedElementLabel { get; set; } = string.Empty;

        /// <summary>
        /// Indique si l'équipement est public.
        /// </summary>
        public bool IsPublicAvailable { get; set; }

        /// <summary>
        /// Indique si l'utilisateur connecté peut modifier cet équipement.
        /// </summary>
        public bool CanEdit { get; set; }
    }
}