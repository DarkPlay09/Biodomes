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
/// PageModel responsable de l'affichage détaillé d'un équipement.
/// Elle récupère l'équipement via son slug, vérifie les droits d'accès,
/// puis prépare les données affichables dans la vue.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IEquipmentRepository _repository;
    private readonly ISlugService _slugService;
    private readonly IEquipmentImageStorage _equipmentImageStorage;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IBiomeRepository _biomeRepository;

    /// <summary>
    /// Initialise la page de détail avec le repository des équipements.
    /// </summary>
    /// <param name="repository">Repository permettant de récupérer un équipement.</param>
    /// <param name="slugService">Service slug pour les liens des équipements.</param>
    /// <param name="equipmentImageStorage">Service responsable de la suppression des images d'équipements.</param>
    /// <param name="userManager">...</param>
    /// <param name="biomeRepository">...</param>
    public DetailsModel(
        IEquipmentRepository repository,
        ISlugService slugService,
        IEquipmentImageStorage equipmentImageStorage,
        UserManager<UserEntity> userManager,
        IBiomeRepository biomeRepository)
    {
        _repository = repository;
        _slugService = slugService;
        _equipmentImageStorage = equipmentImageStorage;
        _userManager = userManager;
        _biomeRepository = biomeRepository;
    }

    /// <summary>
    /// Données détaillées de l'équipement à afficher dans la vue.
    /// </summary>
    public EquipmentDetailsViewModel EquipmentDetails { get; private set; } = new();
    
    /// <summary>
    /// URL de retour vers la page précédemment visitée.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour sécurisée.
    /// </summary>
    public string SafeReturnUrl =>
        Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("./Index") ?? "/equipment";

    /// <summary>
    /// Charge la fiche détaillée d'un équipement à partir de son slug.
    /// L'utilisateur doit être connecté. Les équipements privés ne sont visibles
    /// que par leur créateur.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à afficher.</param>
    /// <returns>La page de détail, une erreur 404, une interdiction ou une demande de connexion.</returns>
    public async Task<IActionResult> OnGetAsync(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var equipment = _repository.GetBySlug(slug);

        if (equipment is null)
        {
            return NotFound();
        }

        var isOwner = equipment.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!equipment.IsPublicAvailable && !isOwner && !isAdmin)
        {
            return Forbid();
        }

        var impactedBiomesCount = _biomeRepository.CountBiomesUsingEquipment(equipment.Id);

        EquipmentDetails = MapToDetails(equipment, isOwner, isAdmin, impactedBiomesCount);

        return Page();
    }

    /// <summary>
    /// Supprime l'équipement courant si l'utilisateur connecté en est le créateur.
    /// L'image associée est également supprimée du stockage.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à supprimer.</param>
    /// <returns>Une redirection vers le catalogue ou une erreur si l'action est interdite.</returns>
    public async Task<IActionResult> OnPostDeleteAsync(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var equipment = _repository.GetBySlug(slug);

        if (equipment is null)
        {
            return NotFound();
        }

        var isOwner = equipment.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!CanManageEquipment(isOwner, isAdmin, equipment.IsPublicAvailable))
        {
            return Forbid();
        }

        var impactedBiomesCount = _biomeRepository.CountBiomesUsingEquipment(equipment.Id);

        if (impactedBiomesCount > 0)
        {
            TempData["ErrorMessage"] =
                $"Suppression impossible : l'équipement « {equipment.Name} » est utilisé dans {impactedBiomesCount} biome(s).";

            return RedirectToPage(new { slug, returnUrl });
        }

        _repository.DeleteBySlug(slug);
        _equipmentImageStorage.Delete(equipment.ImagePath);

        TempData["SuccessMessage"] = $"L'équipement « {equipment.Name} » a bien été supprimé.";

        return LocalRedirect(SafeReturnUrl);
    }

    /// <summary>
    /// Transforme l'entité métier en ViewModel adapté à l'affichage de la page détail.
    /// </summary>
    /// <param name="equipment">Équipement issu du domaine.</param>
    /// <param name="isOwner">Indique si l'utilisateur connecté est le créateur de l'équipement.</param>
    /// <returns>ViewModel prêt à être affiché.</returns>
    private EquipmentDetailsViewModel MapToDetails(
        EquipmentEntity equipment,
        bool isOwner,
        bool isAdmin,
        int impactedBiomesCount)
    {
        return new EquipmentDetailsViewModel
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Slug = _slugService.ToSlug(equipment.Name),
            ImagePath = string.IsNullOrWhiteSpace(equipment.ImagePath)
                ? "/images/equipment/noImageEquipment.png"
                : equipment.ImagePath,
            ReferenceLabel = $"EQ-{equipment.Id:0000}",
            ProducedElementLabel = FormatNullableResource(equipment.ProducedElement),
            ConsumedElementLabel = FormatNullableResource(equipment.ConsumedElement),
            VisibilityLabel = equipment.IsPublicAvailable ? "Publique" : "Privée",
            VisibilityCssClass = equipment.IsPublicAvailable
                ? "species-details-badge--public"
                : "species-details-badge--private",
            OwnerName = string.IsNullOrWhiteSpace(equipment.Creator.UserName)
                ? "Créateur inconnu"
                : equipment.Creator.UserName,
            IsOwner = isOwner,
            CanManage = CanManageEquipment(isOwner, isAdmin, equipment.IsPublicAvailable),
            ImpactedBiomesCount = impactedBiomesCount
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
    /// ViewModel contenant les informations affichées sur la fiche détaillée d'un équipement.
    /// </summary>
    public class EquipmentDetailsViewModel
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
        /// Slug utilisé pour générer les liens vers cet équipement.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Chemin de l'image de l'équipement.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Référence affichable de l'équipement.
        /// </summary>
        public string ReferenceLabel { get; set; } = string.Empty;

        /// <summary>
        /// Ressource produite par l'équipement.
        /// </summary>
        public string ProducedElementLabel { get; set; } = string.Empty;

        /// <summary>
        /// Ressource consommée par l'équipement.
        /// </summary>
        public string ConsumedElementLabel { get; set; } = string.Empty;

        /// <summary>
        /// Libellé indiquant si l'équipement est public ou privé.
        /// </summary>
        public string VisibilityLabel { get; set; } = string.Empty;

        /// <summary>
        /// Classe CSS utilisée pour afficher le badge de visibilité.
        /// </summary>
        public string VisibilityCssClass { get; set; } = string.Empty;

        /// <summary>
        /// Nom du créateur de l'équipement.
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Indique si l'utilisateur connecté est le créateur de l'équipement.
        /// </summary>
        public bool IsOwner { get; set; }
        
        /// <summary>
        /// Indique si l'utilisateur connecté peut modifier ou supprimer cet équipement.
        /// </summary>
        public bool CanManage { get; set; }

        /// <summary>
        /// Nombre de biomes qui utilisent cet équipement.
        /// </summary>
        public int ImpactedBiomesCount { get; set; }
    }
}