using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
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

    /// <summary>
    /// Initialise la page de modification avec les services d'accès aux équipements
    /// et de gestion des images.
    /// </summary>
    /// <param name="equipmentRepository">Repository permettant de lire et modifier les équipements.</param>
    /// <param name="equipmentImageStorage">Service responsable de l'enregistrement et de la suppression des images.</param>
    /// <param name="slugService">Service permettant de générer le slug de l'équipement après sa modification.</param>
    public EditModel(
        IEquipmentRepository equipmentRepository,
        IEquipmentImageStorage equipmentImageStorage,
        ISlugService slugService)
    {
        _equipmentRepository = equipmentRepository;
        _equipmentImageStorage = equipmentImageStorage;
        _slugService = slugService;
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

    /// <summary>
    /// Charge l'équipement à modifier et préremplit le formulaire.
    /// </summary>
    /// <param name="slug">Slug de l'équipement ciblé.</param>
    /// <returns>La page préremplie, une erreur 404, une interdiction ou une demande de connexion.</returns>
    public IActionResult OnGet(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge(); // Aller à la page de connexion.

        var equipment = _equipmentRepository.GetBySlug(slug);
        if (equipment is null)
            return NotFound();

        if (equipment.Creator.Id != currentUserId)
            return Forbid(); // Interdiction de modifier l'équipement d'un autre utilisateur.

        Input.Name = equipment.Name;
        Input.ProducedElement = equipment.ProducedElement?.ToString();
        Input.ConsumedElement = equipment.ConsumedElement?.ToString();
        CurrentImagePath = equipment.ImagePath;

        return Page();
    }

    /// <summary>
    /// Traite l'enregistrement des modifications d'un équipement.
    /// La méthode vérifie les droits, valide les données, remplace éventuellement l'image,
    /// puis met à jour l'équipement via le repository.
    /// </summary>
    /// <param name="slug">Slug de l'équipement à modifier.</param>
    /// <returns>La page avec les erreurs éventuelles ou une redirection vers la liste des équipements.</returns>
    public async Task<IActionResult> OnPost(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var existingEquipment = _equipmentRepository.GetBySlug(slug);
        if (existingEquipment is null)
            return NotFound();

        if (existingEquipment.Creator.Id != currentUserId)
            return Forbid();

        CurrentImagePath = existingEquipment.ImagePath;

        if (!ModelState.IsValid)
            return Page();

        var produced = ParseResourceType(Input.ProducedElement);
        var consumed = ParseResourceType(Input.ConsumedElement);

        if (!ModelState.IsValid)
            return Page();

        // Un équipement doit rester utile dans l'écosystème : produire et/ou consommer une ressource.
        if (produced is null && consumed is null)
        {
            ModelState.AddModelError(string.Empty, "Un equipement doit produire et/ou consommer au moins un element.");
            return Page();
        }

        var oldImagePath = existingEquipment.ImagePath;
        var imagePath = existingEquipment.ImagePath;
        var hasNewImage = false;

        // Si une nouvelle image est fournie, elle remplace l'ancienne.
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

        // Reconstruction de l'objet métier avec les nouvelles valeurs du formulaire.
        var equipment = new Domains.Entities.Equipment(
            Input.Name!,
            produced,
            consumed,
            imagePath,
            new UserAccount { Id = currentUserId },
            existingEquipment.IsPublicAvailable)
        {
            Id = existingEquipment.Id
        };

        _equipmentRepository.Update(slug, equipment);
        
        TempData["SuccessMessage"] = $"L'équipement « {Input.Name} » a bien été modifié.";
        
        var newSlug = _slugService.ToSlug(equipment.Name);

        return RedirectToPage("./Details", new { slug = newSlug });
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