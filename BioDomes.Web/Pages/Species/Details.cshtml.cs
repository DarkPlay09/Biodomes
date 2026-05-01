using System.Globalization;
using System.Security.Claims;
using System.Text;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// PageModel responsable de l'affichage détaillé d'une espèce.
/// Elle récupère l'espèce via son slug, vérifie les droits d'accès,
/// puis prépare les données affichables dans la vue.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly ISpeciesRepository _repository;
    private readonly ISlugService _slugService;
    private readonly ISpeciesImageStorage _speciesImageStorage;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IBiomeRepository _biomeRepository;

    /// <summary>
    /// Initialise la page de détail avec le repository des espèces.
    /// </summary>
    /// <param name="repository">Repository permettant de récupérer une espèce.</param>
    /// <param name="slugService">Service slug pour les liens des espèces.</param>
    /// <param name="speciesImageStorage">Service responsable de la suppression des images d'espèces.</param>
    /// <param name="userManager">...</param>
    /// <param name="biomeRepository">...</param>
    public DetailsModel(
        ISpeciesRepository repository,
        ISlugService slugService,
        ISpeciesImageStorage speciesImageStorage,
        UserManager<UserEntity> userManager,
        IBiomeRepository biomeRepository)
    {
        _repository = repository;
        _slugService = slugService;
        _speciesImageStorage = speciesImageStorage;
        _userManager = userManager;
        _biomeRepository = biomeRepository;
    }

    /// <summary>
    /// Données détaillées de l'espèce à afficher dans la vue.
    /// </summary>
    public SpeciesDetailsViewModel SpeciesDetails { get; private set; } = new();
    
    /// <summary>
    /// URL de retour vers la page précédemment visitée.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour sécurisée.
    /// </summary>
    public string SafeReturnUrl
    {
        get
        {
            var fallbackUrl = Url.Page("./Index") ?? Url.Content("~/species");

            if (string.IsNullOrWhiteSpace(ReturnUrl) || !Url.IsLocalUrl(ReturnUrl))
                return fallbackUrl;

            var pathBase = HttpContext.Request.PathBase.Value;

            if (!string.IsNullOrWhiteSpace(pathBase)
                && ReturnUrl.StartsWith("/")
                && !ReturnUrl.StartsWith(pathBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                return pathBase + ReturnUrl;
            }

            return ReturnUrl;
        }
    }

    /// <summary>
    /// Charge la fiche détaillée d'une espèce à partir de son slug.
    /// L'utilisateur doit être connecté. Les espèces privées ne sont visibles
    /// que par leur créateur.
    /// </summary>
    /// <param name="slug">Slug de l'espèce à afficher.</param>
    /// <returns>La page de détail, une erreur 404, une interdiction ou une demande de connexion.</returns>
    public async Task<IActionResult> OnGetAsync(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var species = _repository.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        var isOwner = species.Creator.Id == currentUserId;
        var isAdmin = await IsCurrentUserAdminAsync();

        if (!species.IsPublicAvailable && !isOwner && !isAdmin)
        {
            return Forbid();
        }

        var impactedBiomesCount = _biomeRepository.CountBiomesUsingSpecies(species.Id);

        SpeciesDetails = MapToDetails(species, isOwner, isAdmin, impactedBiomesCount);

        return Page();
    }
    
    /// <summary>
    /// Vérifie si l'utilisateur connecté est administrateur.
    /// </summary>
    /// <returns>True si l'utilisateur connecté est administrateur, sinon false.</returns>
    private async Task<bool> IsCurrentUserAdminAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Supprime l'espèce courante si l'utilisateur connecté en est le créateur.
    /// L'image associée est également supprimée du stockage.
    /// </summary>
    /// <param name="slug">Slug de l'espèce à supprimer.</param>
    /// <returns>Une redirection vers le catalogue ou une erreur si l'action est interdite.</returns>
    public async Task<IActionResult> OnPostDeleteAsync(string slug, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var species = _repository.GetBySlug(slug);

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

        var impactedBiomesCount = _biomeRepository.CountBiomesUsingSpecies(species.Id);

        if (impactedBiomesCount > 0)
        {
            TempData["ErrorMessage"] =
                $"Suppression impossible : l'espèce « {species.Name} » est utilisée dans {impactedBiomesCount} biome(s).";

            return RedirectToPage(new { slug, returnUrl });
        }

        _repository.DeleteBySlug(slug);
        _speciesImageStorage.Delete(species.ImagePath);

        TempData["SuccessMessage"] = $"L'espèce « {species.Name} » a bien été supprimée.";

        return LocalRedirect(SafeReturnUrl);
    }

    /// <summary>
    /// Transforme l'entité métier en ViewModel adapté à l'affichage de la page détail.
    /// </summary>
    /// <param name="species">Espèce issue du domaine.</param>
    /// <param name="isOwner">Indique si l'utilisateur connecté est le créateur de l'espèce.</param>
    /// <returns>ViewModel prêt à être affiché.</returns>
    private SpeciesDetailsViewModel MapToDetails(SpeciesEntity species, bool isOwner)
    {
        return new SpeciesDetailsViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = _slugService.ToSlug(species.Name),
            ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                ? "/uploads/species/noImageSpecie.png"
                : species.ImagePath,
            ClassificationLabel = FormatClassification(species.Classification),
            DietLabel = FormatDiet(species.Diet),
            AdultSizeLabel = FormatSize(species.AdultSize),
            WeightLabel = FormatWeight(species.Weight),
            VisibilityLabel = species.IsPublicAvailable ? "Publique" : "Privée",
            VisibilityCssClass = species.IsPublicAvailable
                ? "species-details-badge--public"
                : "species-details-badge--private",
            OwnerName = string.IsNullOrWhiteSpace(species.Creator.UserName)
                ? "Créateur inconnu"
                : species.Creator.UserName,
            IsOwner = isOwner
        };
    }

    /// <summary>
    /// Formate une classification du domaine en libellé lisible pour l'interface.
    /// </summary>
    /// <param name="classification">Classification à formater.</param>
    /// <returns>Libellé en français.</returns>
    public string FormatClassification(SpeciesClassification classification)
    {
        return classification switch
        {
            SpeciesClassification.Mammiferes => "Mammifère",
            SpeciesClassification.Oiseaux => "Oiseau",
            SpeciesClassification.Poissons => "Poisson",
            SpeciesClassification.Reptiles => "Reptile",
            SpeciesClassification.Amphibiens => "Amphibien",
            SpeciesClassification.Crustaces => "Crustacé",
            SpeciesClassification.Arachnides => "Arachnide",
            SpeciesClassification.Insectes => "Insecte",
            SpeciesClassification.Mollusques => "Mollusque",
            SpeciesClassification.Plantes => "Plante",
            SpeciesClassification.Mousses => "Mousse",
            SpeciesClassification.Algues => "Algue",
            SpeciesClassification.Lichen => "Lichen",
            SpeciesClassification.Champignons => "Champignon",
            SpeciesClassification.Myriapodes => "Myriapode",
            SpeciesClassification.Echinodermes => "Échinoderme",
            SpeciesClassification.Annelides => "Annélide",
            SpeciesClassification.Cnidaires => "Cnidaire",
            SpeciesClassification.Plathelminthes => "Plathelminthe",
            SpeciesClassification.Poriferes => "Porifère",
            _ => classification.ToString()
        };
    }

    /// <summary>
    /// Formate un régime alimentaire du domaine en libellé lisible pour l'interface.
    /// </summary>
    /// <param name="diet">Régime alimentaire à formater.</param>
    /// <returns>Libellé en français.</returns>
    public string FormatDiet(DietType diet)
    {
        return diet switch
        {
            DietType.Detritivore => "Détritivore",
            DietType.Photosynthese => "Photosynthèse",
            DietType.Heterotrophie => "Hétérotrophie",
            _ => diet.ToString()
        };
    }

    /// <summary>
    /// Génère un slug à partir d'un nom.
    /// Les accents sont retirés et les séparateurs sont remplacés par des tirets.
    /// </summary>
    /// <param name="value">Texte à transformer en slug.</param>
    /// <returns>Slug normalisé.</returns>
    private static string ToSlug(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    /// <summary>
    /// Formate la taille adulte en mètres.
    /// </summary>
    /// <param name="size">Taille adulte numérique.</param>
    /// <returns>Libellé formaté.</returns>
    private static string FormatSize(double size)
    {
        return $"{size:0.##} m";
    }

    /// <summary>
    /// Formate le poids en kilogrammes ou en tonnes selon la valeur.
    /// </summary>
    /// <param name="weight">Poids numérique.</param>
    /// <returns>Libellé formaté.</returns>
    private static string FormatWeight(double weight)
    {
        return weight >= 1000
            ? $"{weight / 1000:0.##} t"
            : $"{weight:0.##} kg";
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
    
    private SpeciesDetailsViewModel MapToDetails(
        SpeciesEntity species,
        bool isOwner,
        bool isAdmin,
        int impactedBiomesCount)
    {
        return new SpeciesDetailsViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = _slugService.ToSlug(species.Name),
            ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                ? "/uploads/species/noImageSpecie.png"
                : species.ImagePath,
            ClassificationLabel = FormatClassification(species.Classification),
            DietLabel = FormatDiet(species.Diet),
            AdultSizeLabel = FormatSize(species.AdultSize),
            WeightLabel = FormatWeight(species.Weight),
            VisibilityLabel = species.IsPublicAvailable ? "Publique" : "Privée",
            VisibilityCssClass = species.IsPublicAvailable
                ? "species-details-badge--public"
                : "species-details-badge--private",
            OwnerName = string.IsNullOrWhiteSpace(species.Creator.UserName)
                ? "Créateur inconnu"
                : species.Creator.UserName,
            IsOwner = isOwner,
            CanManage = CanManageSpecies(isOwner, isAdmin, species.IsPublicAvailable),
            ImpactedBiomesCount = impactedBiomesCount
        };
    }
    
    private static bool CanManageSpecies(bool isOwner, bool isAdmin, bool isPublicAvailable)
    {
        return (isOwner && !isPublicAvailable)
               || (isAdmin && isPublicAvailable);
    }

    /// <summary>
    /// ViewModel contenant les informations affichées sur la fiche détaillée d'une espèce.
    /// </summary>
    public class SpeciesDetailsViewModel
    {
        /// <summary>
        /// Identifiant technique de l'espèce.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nom courant de l'espèce.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Chemin de l'image de l'espèce.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Libellé de la classification.
        /// </summary>
        public string ClassificationLabel { get; set; } = string.Empty;

        /// <summary>
        /// Libellé du régime alimentaire.
        /// </summary>
        public string DietLabel { get; set; } = string.Empty;

        /// <summary>
        /// Taille adulte formatée.
        /// </summary>
        public string AdultSizeLabel { get; set; } = string.Empty;

        /// <summary>
        /// Poids formaté.
        /// </summary>
        public string WeightLabel { get; set; } = string.Empty;

        /// <summary>
        /// Libellé indiquant si l'espèce est publique ou privée.
        /// </summary>
        public string VisibilityLabel { get; set; } = string.Empty;

        /// <summary>
        /// Classe CSS utilisée pour afficher le badge de visibilité.
        /// </summary>
        public string VisibilityCssClass { get; set; } = string.Empty;

        /// <summary>
        /// Nom du créateur de l'espèce.
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Indique si l'utilisateur connecté est le créateur de l'espèce.
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Slug utilisé pour générer les liens vers cette espèce.
        /// </summary>
        public string Slug { get; set; } = string.Empty;
        
        /// <summary>
        /// Indique si l'utilisateur connecté peut modifier ou supprimer cette espèce.
        /// </summary>
        public bool CanManage { get; set; }

        /// <summary>
        /// Nombre de biomes qui utilisent cette espèce.
        /// </summary>
        public int ImpactedBiomesCount { get; set; }
    }
}