using System.Globalization;
using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

/// <summary>
/// PageModel du catalogue des biomes de l'utilisateur connecté.
/// Elle gère l'affichage en cartes, la recherche, les filtres et la pagination.
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// Nombre maximal de biomes affichés par page.
    /// </summary>
    private const int BiomesPerPage = 8;

    private readonly IBiomeRepository _biomeRepository;
    private readonly ISlugService _slugService;

    /// <summary>
    /// Initialise la page des biomes.
    /// </summary>
    /// <param name="biomeRepository">Repository permettant de lire et supprimer les biomes.</param>
    /// <param name="slugService">Service permettant de générer les slugs.</param>
    public IndexModel(
        IBiomeRepository biomeRepository,
        ISlugService slugService)
    {
        _biomeRepository = biomeRepository;
        _slugService = slugService;
    }

    /// <summary>
    /// Texte recherché dans le nom ou l'état du biome.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    /// <summary>
    /// Filtre optionnel sur l'état du biome.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public BiomeState? StateFilter { get; set; }

    /// <summary>
    /// Filtre optionnel sur la température.
    /// Les valeurs attendues sont cold, normal, hot ou vide.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? TemperatureFilter { get; set; }

    /// <summary>
    /// Numéro de page demandé.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Nombre total de biomes avant filtrage.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Nombre de biomes après filtrage.
    /// </summary>
    public int FilteredCount { get; private set; }

    /// <summary>
    /// Nombre de biomes optimaux.
    /// </summary>
    public int OptimalCount { get; private set; }

    /// <summary>
    /// Nombre de biomes instables.
    /// </summary>
    public int InstableCount { get; private set; }

    /// <summary>
    /// Nombre de biomes critiques.
    /// </summary>
    public int CritiqueCount { get; private set; }

    /// <summary>
    /// Nombre total de pages.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Numéros de pages visibles dans la pagination.
    /// </summary>
    public IReadOnlyList<int> VisiblePages { get; private set; } = [];

    /// <summary>
    /// Cartes de biomes affichées sur la page courante.
    /// </summary>
    public IReadOnlyList<BiomeCardViewModel> BiomeCards { get; private set; } = [];

    /// <summary>
    /// Options disponibles pour le filtre d'état.
    /// </summary>
    public IReadOnlyList<BiomeState> StateOptions { get; } =
        Enum.GetValues<BiomeState>();

    /// <summary>
    /// Charge les biomes du créateur connecté, applique les filtres et prépare la pagination.
    /// </summary>
    /// <returns>La page des biomes ou une demande de connexion.</returns>
    public IActionResult OnGet()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Challenge();
        }

        var biomes = _biomeRepository.GetAllByCreator(userId).ToList();

        TotalCount = biomes.Count;
        OptimalCount = biomes.Count(biome => biome.State == BiomeState.Optimal);
        InstableCount = biomes.Count(biome => biome.State == BiomeState.Instable);
        CritiqueCount = biomes.Count(biome => biome.State == BiomeState.Critique);

        var filteredBiomes = ApplyFilters(biomes)
            .OrderByDescending(biome => biome.UpdatedAt)
            .ThenBy(biome => biome.Name)
            .ToList();

        FilteredCount = filteredBiomes.Count;

        TotalPages = Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)BiomesPerPage));

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        BiomeCards = filteredBiomes
            .Skip((PageNumber - 1) * BiomesPerPage)
            .Take(BiomesPerPage)
            .Select(MapToCard)
            .ToList();

        VisiblePages = BuildVisiblePages(PageNumber, TotalPages);

        return Page();
    }

    /// <summary>
    /// Supprime un biome appartenant à l'utilisateur connecté.
    /// </summary>
    /// <param name="slug">Slug du biome à supprimer.</param>
    /// <returns>Une redirection vers la page courante ou une erreur.</returns>
    public IActionResult OnPostDelete(string slug)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Challenge();
        }

        var biome = _biomeRepository.GetBySlug(slug);

        if (biome is null)
        {
            return NotFound();
        }

        if (biome.Creator.Id != userId)
        {
            return Forbid();
        }

        _biomeRepository.DeleteBySlug(slug);

        return RedirectToPage(new
        {
            Search,
            StateFilter,
            TemperatureFilter,
            PageNumber
        });
    }

    /// <summary>
    /// Formate un état de biome en français.
    /// </summary>
    /// <param name="state">État à formater.</param>
    /// <returns>Libellé affichable.</returns>
    public string FormatState(BiomeState state)
    {
        return state switch
        {
            BiomeState.Optimal => "Optimal",
            BiomeState.Instable => "Instable",
            BiomeState.Critique => "Critique",
            _ => state.ToString()
        };
    }

    /// <summary>
    /// Applique la recherche et les filtres sélectionnés.
    /// </summary>
    /// <param name="biomes">Biomes avant filtrage.</param>
    /// <returns>Biomes filtrés.</returns>
    private IEnumerable<Domains.Entities.Biome> ApplyFilters(IEnumerable<Domains.Entities.Biome> biomes)
    {
        var filteredBiomes = biomes;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            filteredBiomes = filteredBiomes.Where(biome =>
                biome.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || FormatState(biome.State).Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (StateFilter.HasValue)
        {
            filteredBiomes = filteredBiomes.Where(biome => biome.State == StateFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(TemperatureFilter))
        {
            var temperatureFilter = TemperatureFilter.Trim().ToLowerInvariant();

            filteredBiomes = temperatureFilter switch
            {
                "cold" => filteredBiomes.Where(biome => biome.Temperature < 0),
                "normal" => filteredBiomes.Where(biome => biome.Temperature >= 0 && biome.Temperature <= 30),
                "hot" => filteredBiomes.Where(biome => biome.Temperature > 30),
                _ => filteredBiomes
            };
        }

        return filteredBiomes;
    }

    /// <summary>
    /// Convertit un biome en carte affichable.
    /// </summary>
    /// <param name="biome">Biome à convertir.</param>
    /// <returns>Carte de biome.</returns>
    private BiomeCardViewModel MapToCard(Domains.Entities.Biome biome)
    {
        var fr = CultureInfo.GetCultureInfo("fr-BE");

        var temperatureCssClass = GetTemperatureCssClass(biome.Temperature);
        var temperatureIconPath = GetTemperatureIconPath(biome.Temperature);

        // Si tu n'as pas d'humidité relative en %, on convertit l'humidité absolue
        // en pourcentage visuel.
        // 30 g/m³ est une base raisonnable pour un remplissage visuel.
        var humidityPercent = Math.Clamp(
            (int)Math.Round((biome.AbsoluteHumidity / 30d) * 100),
            0,
            100);

        return new BiomeCardViewModel
        {
            Id = biome.Id,
            Name = biome.Name,
            Slug = _slugService.ToSlug(biome.Name),
            TemperatureLabel = $"{biome.Temperature.ToString("0.0", fr)} °C",
            AbsoluteHumidityLabel = $"{biome.AbsoluteHumidity.ToString("0.00", fr)} g/m³",
            TemperatureCssClass = temperatureCssClass,
            TemperatureIconPath = temperatureIconPath,
            HumidityPercent = humidityPercent,
            State = biome.State,
            StateLabel = FormatState(biome.State),
            StateCssClass = biome.State switch
            {
                BiomeState.Optimal => "biome-card--optimal",
                BiomeState.Instable => "biome-card--unstable",
                BiomeState.Critique => "biome-card--critical",
                _ => "biome-card--optimal"
            },
            Score = biome.State switch
            {
                BiomeState.Optimal => 92,
                BiomeState.Instable => 45,
                BiomeState.Critique => 21,
                _ => 60
            },
            LastUpdatedLabel = biome.UpdatedAt
                .ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm", fr)
        };
    }
    
    private static string GetTemperatureCssClass(double temperature)
    {
        if (temperature < 0)
        {
            return "biome-card__metric--cold";
        }

        if (temperature > 30)
        {
            return "biome-card__metric--hot";
        }

        return "biome-card__metric--stable";
    }

    private static string GetTemperatureIconPath(double temperature)
    {
        if (temperature < 0)
        {
            return "~/images/biomes/icons/temperature-cold.png";
        }

        if (temperature > 30)
        {
            return "~/images/biomes/icons/temperature-hot.png";
        }

        return "~/images/biomes/icons/temperature-stable.png";
    }

    /// <summary>
    /// Calcule les numéros de pages visibles dans la pagination.
    /// </summary>
    /// <param name="currentPage">Page courante.</param>
    /// <param name="totalPages">Nombre total de pages.</param>
    /// <returns>Liste des pages visibles.</returns>
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
    /// Récupère l'identifiant de l'utilisateur connecté.
    /// </summary>
    /// <param name="userId">Identifiant utilisateur.</param>
    /// <returns>True si l'identifiant est valide.</returns>
    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(claim, out userId) && userId > 0;
    }

    /// <summary>
    /// ViewModel d'une carte biome.
    /// </summary>
    public class BiomeCardViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string TemperatureLabel { get; set; } = string.Empty;

        public string AbsoluteHumidityLabel { get; set; } = string.Empty;

        public BiomeState State { get; set; }

        public string StateLabel { get; set; } = string.Empty;

        public string StateCssClass { get; set; } = string.Empty;

        public int Score { get; set; }

        public string LastUpdatedLabel { get; set; } = string.Empty;
        
        public string TemperatureCssClass { get; set; } = string.Empty;

        public string TemperatureIconPath { get; set; } = string.Empty;

        public int HumidityPercent { get; set; }
    }
}