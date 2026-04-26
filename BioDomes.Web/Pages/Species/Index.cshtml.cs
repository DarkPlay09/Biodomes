using System.Globalization;
using System.Security.Claims;
using System.Text;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SpeciesEntity = BioDomes.Domains.Entities.Species;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// PageModel du catalogue des espèces.
/// Elle gère l'affichage des espèces visibles par l'utilisateur connecté,
/// les filtres, la recherche, la pagination et la suppression.
/// </summary>
public class SpeciesModel : PageModel
{
    /// <summary>
    /// Nombre maximal d'espèces affichées par page.
    /// </summary>
    private const int SpeciesPerPage = 8;

    private readonly ISpeciesRepository _repository;
    private readonly IWebHostEnvironment _environment;
    private readonly ISlugService _slugService;

    /// <summary>
    /// Initialise la page catalogue avec le repository des espèces
    /// et l'environnement web utilisé pour supprimer les images physiques.
    /// </summary>
    /// <param name="repository">Repository permettant de lire et supprimer les espèces.</param>
    /// <param name="environment">Environnement web donnant accès au dossier wwwroot.</param>
    /// <param name="slugService">Service slug pour les liens des espèces.</param>
    public SpeciesModel(
        ISpeciesRepository repository,
        IWebHostEnvironment environment,
        ISlugService slugService)
    {
        _repository = repository;
        _environment = environment;
        _slugService = slugService;
    }

    /// <summary>
    /// Message temporaire affiché lorsqu'une espèce vient d'être ajoutée.
    /// </summary>
    [TempData]
    public string? LastInsertedSpeciesName { get; set; }

    /// <summary>
    /// Texte recherché dans le nom, la classification ou le régime alimentaire.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    /// <summary>
    /// Filtre optionnel sur la classification de l'espèce.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public SpeciesClassification? ClassificationFilter { get; set; }

    /// <summary>
    /// Filtre optionnel sur le régime alimentaire.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public DietType? DietFilter { get; set; }

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
    /// Nombre d'espèces affichées par page.
    /// </summary>
    public int PageSize => SpeciesPerPage;

    /// <summary>
    /// Nombre total d'espèces visibles avant filtrage.
    /// </summary>
    public int TotalSpeciesCount { get; private set; }

    /// <summary>
    /// Nombre total d'espèces après application des filtres.
    /// </summary>
    public int FilteredSpeciesCount { get; private set; }

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
    /// Liste des espèces converties en cartes pour l'affichage.
    /// </summary>
    public IReadOnlyList<SpeciesCatalogItemViewModel> SpeciesCards { get; private set; } = [];

    /// <summary>
    /// Numéros de pages affichés dans la pagination.
    /// </summary>
    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    /// <summary>
    /// Options disponibles pour le filtre de classification.
    /// </summary>
    public IReadOnlyList<SpeciesClassification> ClassificationOptions { get; } =
        Enum.GetValues<SpeciesClassification>();

    /// <summary>
    /// Options disponibles pour le filtre de régime alimentaire.
    /// </summary>
    public IReadOnlyList<DietType> DietOptions { get; } =
        Enum.GetValues<DietType>();

    /// <summary>
    /// Charge les espèces visibles, applique les filtres,
    /// calcule la pagination et prépare les cartes du catalogue.
    /// </summary>
    /// <returns>La page catalogue ou une demande de connexion.</returns>
    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        // L'utilisateur voit les espèces publiques ainsi que ses propres espèces privées.
        var visibleSpecies = _repository.GetAll()
            .Where(species => species.IsPublicAvailable || species.Creator.Id == currentUserId)
            .ToList();

        TotalSpeciesCount = visibleSpecies.Count;

        var filteredSpecies = ApplyFilters(visibleSpecies)
            .OrderBy(species => species.Name)
            .ToList();

        FilteredSpeciesCount = filteredSpecies.Count;

        TotalPages = Math.Max(1, (int)Math.Ceiling(FilteredSpeciesCount / (double)SpeciesPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        FirstItemNumber = FilteredSpeciesCount == 0
            ? 0
            : ((PageNumber - 1) * SpeciesPerPage) + 1;

        LastItemNumber = Math.Min(PageNumber * SpeciesPerPage, FilteredSpeciesCount);

        SpeciesCards = filteredSpecies
            .Skip((PageNumber - 1) * SpeciesPerPage)
            .Take(SpeciesPerPage)
            .Select(species => MapToCatalogItem(species, currentUserId))
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    /// <summary>
    /// Supprime une espèce appartenant à l'utilisateur connecté.
    /// L'image associée est également supprimée du dossier wwwroot si elle existe.
    /// </summary>
    /// <param name="slug">Slug de l'espèce à supprimer.</param>
    /// <returns>Une redirection vers la page courante ou une erreur si l'action est impossible.</returns>
    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Challenge();
        }

        var species = _repository.GetBySlug(slug);

        if (species is null)
        {
            return NotFound();
        }

        if (species.Creator.Id != currentUserId)
        {
            return Forbid();
        }

        _repository.DeleteBySlug(slug);

        // Suppression de l'image physique uniquement si elle correspond à une image uploadée.
        if (!string.IsNullOrWhiteSpace(species.ImagePath)
            && species.ImagePath.StartsWith("/images/species/")
            && species.ImagePath != "/images/species/noImageSpecie.png")
        {
            var relativePath = species.ImagePath
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Formate une classification du domaine en libellé français lisible.
    /// </summary>
    /// <param name="classification">Classification à formater.</param>
    /// <returns>Libellé affichable.</returns>
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
    /// Formate un régime alimentaire du domaine en libellé français lisible.
    /// </summary>
    /// <param name="diet">Régime alimentaire à formater.</param>
    /// <returns>Libellé affichable.</returns>
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
    /// Applique la recherche et les filtres sélectionnés par l'utilisateur.
    /// </summary>
    /// <param name="species">Liste d'espèces visibles avant filtrage.</param>
    /// <returns>Liste filtrée.</returns>
    private IEnumerable<SpeciesEntity> ApplyFilters(IEnumerable<SpeciesEntity> species)
    {
        var filteredSpecies = species;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatClassification(currentSpecies.Classification).Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatDiet(currentSpecies.Diet).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (ClassificationFilter.HasValue)
        {
            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Classification == ClassificationFilter.Value);
        }

        if (DietFilter.HasValue)
        {
            filteredSpecies = filteredSpecies.Where(currentSpecies =>
                currentSpecies.Diet == DietFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(VisibilityFilter))
        {
            var visibility = VisibilityFilter.Trim().ToLowerInvariant();

            filteredSpecies = visibility switch
            {
                "public" => filteredSpecies.Where(currentSpecies => currentSpecies.IsPublicAvailable),
                "private" => filteredSpecies.Where(currentSpecies => !currentSpecies.IsPublicAvailable),
                _ => filteredSpecies
            };
        }

        return filteredSpecies;
    }

    /// <summary>
    /// Convertit une espèce du domaine en ViewModel de carte catalogue.
    /// </summary>
    /// <param name="species">Espèce à afficher.</param>
    /// <param name="currentUserId">Identifiant de l'utilisateur connecté.</param>
    /// <returns>Carte prête à être affichée.</returns>
    private SpeciesCatalogItemViewModel MapToCatalogItem(SpeciesEntity species, int currentUserId)
    {
        return new SpeciesCatalogItemViewModel
        {
            Id = species.Id,
            Name = species.Name,
            Slug = _slugService.ToSlug(species.Name),
            ImagePath = string.IsNullOrWhiteSpace(species.ImagePath)
                ? "/images/species/noImageSpecie.png"
                : species.ImagePath,
            ClassificationLabel = FormatClassification(species.Classification),
            DietLabel = FormatDiet(species.Diet),
            AdultSizeLabel = FormatSize(species.AdultSize),
            WeightLabel = FormatWeight(species.Weight),
            IsPublicAvailable = species.IsPublicAvailable,
            CanEdit = species.Creator.Id == currentUserId
        };
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
    /// Génère un slug à partir d'un nom.
    /// Les accents sont retirés et les séparateurs sont remplacés par des tirets.
    /// </summary>
    /// <param name="value">Texte à transformer en slug.</param>
    /// <returns>Slug normalisé.</returns>
    private static string ToSlug(string value) //TODO : cette fonction n'est pas utilisée
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
    /// ViewModel représentant une espèce affichée dans le catalogue.
    /// </summary>
    public class SpeciesCatalogItemViewModel
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
        /// Slug utilisé pour générer les liens vers les pages détails ou modification.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Chemin de l'image affichée sur la carte.
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
        /// Indique si l'espèce est publique.
        /// </summary>
        public bool IsPublicAvailable { get; set; }

        /// <summary>
        /// Indique si l'utilisateur connecté peut modifier cette espèce.
        /// </summary>
        public bool CanEdit { get; set; }
    }
}