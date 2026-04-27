using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// PageModel responsable de la modification d'un équipement existant.
/// Elle vérifie que l'utilisateur connecté est bien le créateur de l'équipement
/// avant d'autoriser l'affichage ou l'enregistrement des modifications.
/// </summary>
public class EditModel : PageModel
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;
    private readonly ISlugService _slugService;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IBiomeRepository _biomeRepository;

    /// <summary>
    /// Initialise la page de modification avec les services d'accès aux équipements
    /// et de gestion des images.
    /// </summary>
    /// <param name="equipmentRepository">Repository permettant de lire et modifier les équipements.</param>
    /// <param name="equipmentImageStorage">Service responsable de l'enregistrement et de la suppression des images.</param>
    /// <param name="slugService">Service permettant de générer le slug de l'équipement après sa modification.</param>
    /// <param name="userManager">...</param>
    /// <param name="biomeRepository">...</param>
    public EditModel(
        IEquipmentRepository equipmentRepository,
        IEquipmentImageStorage equipmentImageStorage,
        ISlugService slugService,
        UserManager<UserEntity> userManager,
        IBiomeRepository biomeRepository)
    {
        _equipmentRepository = equipmentRepository;
        _equipmentImageStorage = equipmentImageStorage;
        _slugService = slugService;
        _userManager = userManager;
        _biomeRepository = biomeRepository;
    }

    /// <summary>
    /// Données du formulaire de modification.
    /// </summary>
    [BindProperty]
    public EquipmentInputModel Input { get; set; } = new();

    /// <summary>
    /// Liste des ressources disponibles pour les listes déroulantes du formulaire.
    /// </summary>
    public IEnumerable<SelectListItem> ResourceOptions =>
        Enum.GetNames<ResourceType>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// Chemin de l'image actuellement associée à l'équipement.
    /// Utilisé pour afficher l'aperçu dans le formulaire.
    /// </summary>
    public string? CurrentImagePath { get; set; }

    /// <summary>
    /// URL de retour vers la page précédente.
    /// </summary>
    [BindProperty]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour sécurisée.
    /// </summary>
    public string SafeReturnUrl =>
        Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("/Equipment/Index") ?? "/equipment";

    public int ImpactedBiomesCount { get; private set; }

    public bool ShowImpactWarning => ImpactedBiomesCount > 0;

    /// <summary>
    /// Charge l'équipement à modifier et préremplit le formulaire.
    /// </summary>
    /// <param name="slug">Slug de l'équipement ciblé.</param>
    /// <returns>La page préremplie, une erreur 404, une interdiction ou une demande de connexion.</returns>
    public async Task<IActionResult> OnGetAsync(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var equipment = _equipmentRepository.GetBySlug(slug);

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

        Input.Name = equipment.Name;
        Input.ProducedElement = equipment.ProducedElement?.ToString();
        Input.ConsumedElement = equipment.ConsumedElement?.ToString();
        CurrentImagePath = equipment.ImagePath;
        ImpactedBiomesCount = _biomeRepository.CountBiomesUsingEquipment(equipment.Id);

        return Page();
    }

    /// <summary>
    /// Traite l'enregistrement des modifications d'un équipement.
    /// La méthode vérifie les droits, valide les données, remplace éventuellement l'image,
    /// puis met à jour l'équipement via le repository.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à modifier.</param>
    /// <returns>La page avec les erreurs éventuelles ou une redirection vers la liste des équipements.</returns>
    public async Task<IActionResult> OnPostAsync(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var existingEquipment = _equipmentRepository.GetBySlug(slug);

        if (existingEquipment is null)
        {
            return NotFound();
        }

        var isOwner = existingEquipment.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!CanManageEquipment(isOwner, isAdmin, existingEquipment.IsPublicAvailable))
        {
            return Forbid();
        }

        CurrentImagePath = existingEquipment.ImagePath;
        ImpactedBiomesCount = _biomeRepository.CountBiomesUsingEquipment(existingEquipment.Id);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var produced = ParseResourceType(Input.ProducedElement);
        var consumed = ParseResourceType(Input.ConsumedElement);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (produced is null && consumed is null)
        {
            ModelState.AddModelError(string.Empty, "Un équipement doit produire et/ou consommer au moins un élément.");
            return Page();
        }

        var producedChanged = existingEquipment.ProducedElement != produced;
        var consumedChanged = existingEquipment.ConsumedElement != consumed;

        var oldImagePath = existingEquipment.ImagePath;
        var imagePath = existingEquipment.ImagePath;
        var hasNewImage = false;

        if (Input.ImageFile is not null)
        {
            try
            {
                imagePath = await _equipmentImageStorage.SaveAsync(
                    Input.Name!,
                    Input.ImageFile.FileName,
                    Input.ImageFile.OpenReadStream());

                hasNewImage = true;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Input.ImageFile", ex.Message);
                return Page();
            }
        }

        if (hasNewImage)
        {
            _equipmentImageStorage.Delete(oldImagePath);
        }

        var equipment = new Domains.Entities.Equipment(
            Input.Name!,
            produced,
            consumed,
            imagePath,
            existingEquipment.Creator,
            existingEquipment.IsPublicAvailable)
        {
            Id = existingEquipment.Id
        };

        _equipmentRepository.Update(slug, equipment);

        if (ImpactedBiomesCount > 0 && (producedChanged || consumedChanged))
        {
            TempData["WarningMessage"] =
                $"Attention : cet équipement est utilisé dans {ImpactedBiomesCount} biome(s). " +
                "Modifier l'élément produit ou consommé peut impacter ces biomes.";
        }

        TempData["SuccessMessage"] = $"L'équipement « {Input.Name} » a bien été modifié.";

        var newSlug = _slugService.ToSlug(equipment.Name);

        return RedirectToPage("./Details", new
        {
            slug = newSlug,
            returnUrl = SafeReturnUrl
        });
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
    /// Convertit une valeur texte du formulaire en <see cref="ResourceType"/>.
    /// </summary>
    /// <param name="rawValue">Valeur textuelle à convertir.</param>
    /// <returns>Le type de ressource correspondant, ou null si aucune valeur n'a été fournie.</returns>
    private ResourceType? ParseResourceType(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        if (Enum.TryParse<ResourceType>(rawValue, out var parsed))
            return parsed;

        ModelState.AddModelError(string.Empty, $"Ressource invalide: {rawValue}.");
        return null;
    }

    /// <summary>
    /// Récupère l'identifiant de l'utilisateur connecté depuis les claims Identity.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur si la récupération réussit.</param>
    /// <returns>True si un identifiant valide a été trouvé, sinon false.</returns>
    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}