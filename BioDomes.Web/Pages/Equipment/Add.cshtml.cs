using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// PageModel responsable de la création d'un nouvel équipement.
/// Elle valide les données du formulaire, enregistre l'image associée,
/// puis délègue l'ajout de l'équipement au repository du domaine.
/// </summary>
public class AddModel : PageModel
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;

    /// <summary>
    /// Initialise la page d'ajout avec les services nécessaires à la persistance
    /// de l'équipement et au stockage de son image.
    /// </summary>
    /// <param name="equipmentRepository">Repository permettant de créer l'équipement.</param>
    /// <param name="equipmentImageStorage">Service responsable de l'enregistrement des images d'équipement.</param>
    public AddModel(IEquipmentRepository equipmentRepository, IEquipmentImageStorage equipmentImageStorage)
    {
        _equipmentRepository = equipmentRepository;
        _equipmentImageStorage = equipmentImageStorage;
    }

    /// <summary>
    /// Données saisies dans le formulaire d'ajout.
    /// </summary>
    [BindProperty]
    public EquipmentInputModel Input { get; set; } = new();

    /// <summary>
    /// Liste des types de ressources disponibles pour les listes déroulantes du formulaire.
    /// </summary>
    public IEnumerable<SelectListItem> ResourceOptions =>
        Enum.GetNames<ResourceType>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// Affiche simplement le formulaire d'ajout d'équipement.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Traite la soumission du formulaire d'ajout.
    /// La méthode valide les données, récupère l'utilisateur connecté,
    /// sauvegarde l'image puis crée l'équipement dans le domaine.
    /// </summary>
    /// <returns>La page avec les erreurs éventuelles ou une redirection vers la liste des équipements.</returns>
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // Une image est obligatoire lors de la création d'un équipement.
        if (Input.ImageFile is null)
        {
            ModelState.AddModelError("Input.ImageFile", "Une image est requise.");
            return Page();
        }

        // Conversion des valeurs du formulaire vers l'enum du domaine.
        var produced = ParseResourceType(Input.ProducedElement);
        var consumed = ParseResourceType(Input.ConsumedElement);

        if (!ModelState.IsValid)
            return Page();

        // Un équipement doit avoir au moins un impact : produire ou consommer une ressource.
        if (produced is null && consumed is null)
        {
            ModelState.AddModelError(string.Empty, "Un équipement doit produire et/ou consommer au moins un élément.");
            return Page();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            return Challenge();

        string? imagePath;
        try
        {
            imagePath = await _equipmentImageStorage.SaveAsync(
                Input.Name!,
                Input.ImageFile.FileName,
                Input.ImageFile.OpenReadStream());
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.ImageFile", ex.Message);
            return Page();
        }

        // Création de l'objet métier avec l'utilisateur courant comme créateur.
        var equipment = new Domains.Entities.Equipment(
            Input.Name!,
            produced,
            consumed,
            imagePath,
            new UserAccount { Id = userId },
            isPublicAvailable: false
        );

        try
        {
            _equipmentRepository.Add(equipment);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.Name", ex.Message);
            return Page();
        }

        return RedirectToPage("/Equipment/Index");
    }

    /// <summary>
    /// Convertit une valeur texte issue du formulaire en <see cref="ResourceType"/>.
    /// Une valeur vide est acceptée, car un équipement peut ne rien produire
    /// ou ne rien consommer tant que l'autre champ est rempli.
    /// </summary>
    /// <param name="rawValue">Valeur textuelle du formulaire.</param>
    /// <returns>Le type de ressource correspondant ou null si le champ est vide.</returns>
    private ResourceType? ParseResourceType(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        if (Enum.TryParse<ResourceType>(rawValue, out var parsed))
            return parsed;

        ModelState.AddModelError(string.Empty, $"Ressource invalide: {rawValue}.");
        return null;
    }
}