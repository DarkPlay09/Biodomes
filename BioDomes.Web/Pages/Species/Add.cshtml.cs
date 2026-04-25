using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// PageModel responsable de l'ajout d'une nouvelle espèce.
/// Elle gère l'affichage du formulaire, la validation des données,
/// l'enregistrement de l'image et la création de l'espèce via le repository.
/// </summary>
public class AddModel : PageModel
{
    private readonly ISpeciesRepository _repo;
    private readonly ISpeciesImageStorage _speciesImageStorage;

    /// <summary>
    /// Initialise la page d'ajout avec le repository des espèces
    /// et le service de stockage des images.
    /// </summary>
    /// <param name="repo">Repository permettant de créer une espèce.</param>
    /// <param name="speciesImageStorage">Service responsable de l'enregistrement des images d'espèces.</param>
    public AddModel(ISpeciesRepository repo, ISpeciesImageStorage speciesImageStorage)
    {
        _repo = repo;
        _speciesImageStorage = speciesImageStorage;
    }

    /// <summary>
    /// Données saisies dans le formulaire d'ajout.
    /// </summary>
    [BindProperty]
    public SpeciesInputModel Input { get; set; } = new();

    /// <summary>
    /// Message temporaire contenant le nom de la dernière espèce ajoutée.
    /// Il peut être affiché après redirection vers la page catalogue.
    /// </summary>
    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    /// <summary>
    /// Options disponibles pour la classification de l'espèce.
    /// Elles sont générées depuis l'énumération du domaine.
    /// </summary>
    public IEnumerable<SelectListItem> ClassificationOptions =>
        Enum.GetNames<SpeciesClassification>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// Options disponibles pour le régime alimentaire de l'espèce.
    /// Elles sont générées depuis l'énumération du domaine.
    /// </summary>
    public IEnumerable<SelectListItem> DietOptions =>
        Enum.GetNames<DietType>()
            .Select(x => new SelectListItem(x, x));

    /// <summary>
    /// URL locale vers laquelle revenir après l'ajout ou via le bouton retour.
    /// Elle est alimentée depuis la query string ou depuis l'en-tête Referer.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour sécurisée.
    /// Si ReturnUrl n'est pas une URL locale valide, on revient vers le catalogue des espèces.
    /// </summary>
    public string SafeReturnUrl =>
        Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("/Species/Index")!;

    /// <summary>
    /// Affiche le formulaire d'ajout.
    /// Si aucune ReturnUrl n'est fournie, la méthode tente d'utiliser la page précédente
    /// via l'en-tête HTTP Referer.
    /// </summary>
    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl))
        {
            return;
        }

        var referer = Request.Headers.Referer.ToString();

        if (!string.IsNullOrWhiteSpace(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            ReturnUrl = refererUri.PathAndQuery;
        }
    }

    /// <summary>
    /// Traite la soumission du formulaire d'ajout d'espèce.
    /// La méthode valide l'utilisateur connecté, l'image, les énumérations,
    /// puis crée l'espèce dans le domaine.
    /// </summary>
    /// <returns>
    /// La page avec les erreurs éventuelles, une demande de connexion,
    /// ou une redirection vers l'URL de retour.
    /// </returns>
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        // Une image est obligatoire lors de la création d'une espèce.
        if (Input.ImageFile is null)
        {
            ModelState.AddModelError("Input.ImageFile", "Une image est requise.");
            return Page();
        }

        string? imagePath;
        try
        {
            imagePath = await _speciesImageStorage.SaveAsync(
                Input.Name!,
                Input.ImageFile.FileName,
                Input.ImageFile.OpenReadStream());
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.ImageFile", ex.Message);
            return Page();
        }

        // Conversion de la classification reçue depuis le formulaire vers l'enum du domaine.
        if (!Enum.TryParse<SpeciesClassification>(Input.Classification, out var classification))
        {
            ModelState.AddModelError("Input.Classification", "Classification invalide.");
            return Page();
        }

        // Conversion du régime alimentaire reçu depuis le formulaire vers l'enum du domaine.
        if (!Enum.TryParse<DietType>(Input.Diet, out var diet))
        {
            ModelState.AddModelError("Input.Diet", "Régime alimentaire invalide.");
            return Page();
        }

        // Création de l'objet métier avec l'utilisateur connecté comme créateur.
        var species = new Domains.Entities.Species(
            Input.Name!,
            classification,
            diet,
            Input.AdultSize,
            Input.Weight,
            imagePath,
            new UserAccount { Id = currentUserId },
            isPublicAvailable: false);

        try
        {
            _repo.Add(species);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("Input.Name", "Une espèce avec ce nom existe déjà.");
            return Page();
        }

        LastInsertedSpeciesName = Input.Name;

        if (Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Species/Index");
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
}