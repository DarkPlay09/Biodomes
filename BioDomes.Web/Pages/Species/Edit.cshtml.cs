using System.Security.Claims;
using BioDomes.Infrastructures.Services.Slug;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// PageModel responsable de la modification d'une espèce existante.
/// Elle vérifie que l'utilisateur connecté est bien le créateur de l'espèce
/// avant d'autoriser l'affichage ou l'enregistrement des changements.
/// </summary>
public class EditModel : PageModel
{
    private readonly ISpeciesRepository _repo;
    private readonly ISpeciesImageStorage _speciesImageStorage;
    private readonly ISlugService _slugService;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IBiomeRepository _biomeRepository;

    /// <summary>
    /// Initialise la page de modification avec le repository des espèces
    /// et le service de stockage des images.
    /// </summary>
    /// <param name="repo">Repository permettant de récupérer et modifier une espèce.</param>
    /// <param name="speciesImageStorage">Service responsable de l'enregistrement et de la suppression des images.</param>
    /// <param name="slugService">Service permettant de générer le slug de l'espèce après sa modification.</param>
    /// <param name="userManager">...</param>
    /// <param name="biomeRepository">...</param>
    public EditModel(
        ISpeciesRepository repo,
        ISpeciesImageStorage speciesImageStorage,
        ISlugService slugService,
        UserManager<UserEntity> userManager,
        IBiomeRepository biomeRepository)
    {
        _repo = repo;
        _speciesImageStorage = speciesImageStorage;
        _slugService = slugService;
        _userManager = userManager;
        _biomeRepository = biomeRepository;
    }

    /// <summary>
    /// Données du formulaire de modification.
    /// </summary>
    [BindProperty]
    public SpeciesInputModel Input { get; set; } = new();

    /// <summary>
    /// Options disponibles pour la classification de l'espèce.
    /// </summary>
    public IEnumerable<SelectListItem> ClassificationOptions { get; } =
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// Options disponibles pour le régime alimentaire de l'espèce.
    /// </summary>
    public IEnumerable<SelectListItem> DietOptions =>
        Enum.GetNames<DietType>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// Chemin de l'image actuellement enregistrée.
    /// Utilisé pour afficher un aperçu dans le formulaire.
    /// </summary>
    public string? CurrentImagePath { get; set; }

    /// <summary>
    /// URL locale vers laquelle revenir après modification ou via le bouton retour.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour sécurisée.
    /// Si ReturnUrl n'est pas locale, on revient vers le catalogue des espèces.
    /// </summary>
    public string SafeReturnUrl =>
        Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("/Species/Index")!;
    
    public int ImpactedBiomesCount { get; private set; }

    public bool ShowImpactWarning => ImpactedBiomesCount > 0;

    /// <summary>
    /// Charge l'espèce à modifier et préremplit le formulaire.
    /// Seul le créateur de l'espèce peut accéder à cette page.
    /// </summary>
    /// <param name="slug">Slug de l'espèce à modifier.</param>
    /// <returns>La page préremplie, une erreur 404, une interdiction ou une demande de connexion.</returns>
    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var species = _repo.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        var isOwner = species.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!CanManageSpecies(isOwner, isAdmin, species.IsPublicAvailable))
        {
            return Forbid();
        }

        Input.Name = species.Name;
        Input.Classification = species.Classification.ToString();
        Input.Diet = species.Diet.ToString();
        Input.AdultSize = species.AdultSize;
        Input.Weight = species.Weight;
        CurrentImagePath = species.ImagePath;
        ImpactedBiomesCount = _biomeRepository.CountBiomesUsingSpecies(species.Id);

        return Page();
    }

    /// <summary>
    /// Traite la soumission du formulaire de modification.
    /// La méthode vérifie les droits, valide les données,
    /// remplace éventuellement l'image et met à jour l'espèce.
    /// </summary>
    /// <param name="slug">Slug de l'espèce à modifier.</param>
    /// <returns>La page avec erreurs éventuelles ou une redirection vers l'URL de retour.</returns>
    public async Task<IActionResult> OnPost(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        var existingSpecies = _repo.GetBySlug(slug);
        if (existingSpecies is null)
            return NotFound();

        var isOwner = existingSpecies.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!CanManageSpecies(isOwner, isAdmin, existingSpecies.IsPublicAvailable))
        {
            return Forbid();
        }

        ImpactedBiomesCount = _biomeRepository.CountBiomesUsingSpecies(existingSpecies.Id);

        CurrentImagePath = existingSpecies.ImagePath;

        if (!ModelState.IsValid)
            return Page();

        var oldImagePath = existingSpecies.ImagePath;
        var imagePath = existingSpecies.ImagePath;
        var hasNewImage = false;

        // Si une nouvelle image est fournie, elle est enregistrée et remplacera l'ancienne.
        if (Input.ImageFile is not null)
        {
            try
            {
                imagePath = await _speciesImageStorage.SaveAsync(
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

        // L'ancienne image est supprimée uniquement après la réussite de l'enregistrement de la nouvelle.
        if (hasNewImage)
        {
            _speciesImageStorage.Delete(oldImagePath);
        }

        if (!Enum.TryParse<SpeciesClassification>(Input.Classification, out var classification))
        {
            ModelState.AddModelError("Input.Classification", "Classification invalide.");
            return Page();
        }

        if (!Enum.TryParse<DietType>(Input.Diet, out var diet))
        {
            ModelState.AddModelError("Input.Diet", "Regime alimentaire invalide.");
            return Page();
        }

        // Reconstruction de l'objet métier avec les nouvelles valeurs du formulaire.
        var species = new Domains.Entities.Species(
            Input.Name!,
            classification,
            diet,
            Input.AdultSize,
            Input.Weight,
            imagePath,
            existingSpecies.Creator,
            existingSpecies.IsPublicAvailable)
        {
            Id = existingSpecies.Id
        };

        _repo.Update(slug, species);
        
        var dietChanged = existingSpecies.Diet != diet;
        var weightChanged = Math.Abs(existingSpecies.Weight - Input.Weight) > 0.0001;

        if (ImpactedBiomesCount > 0 && (dietChanged || weightChanged))
        {
            TempData["WarningMessage"] =
                $"Attention : cette espèce est utilisée dans {ImpactedBiomesCount} biome(s). " +
                "Modifier son poids ou son régime alimentaire peut impacter ces biomes.";
        }

        TempData["SuccessMessage"] = $"L'espèce « {Input.Name} » a bien été modifiée.";

        var newSlug = _slugService.ToSlug(Input.Name!);

        return RedirectToPage("./Details", new
        {
            slug = newSlug,
            returnUrl = SafeReturnUrl
        });
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
    
    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanManageSpecies(bool isOwner, bool isAdmin, bool isPublicAvailable)
    {
        return (isOwner && !isPublicAvailable)
               || (isAdmin && isPublicAvailable);
    }
}