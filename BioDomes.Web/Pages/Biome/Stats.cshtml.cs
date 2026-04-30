using System.Security.Claims;
using BioDomes.Web.Services.Stats;
using BioDomes.Web.ViewModels.Stats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

/// <summary>
/// PageModel responsable de l'affichage des statistiques d'un biome précis.
/// La page est accessible uniquement aux utilisateurs authentifiés.
/// </summary>
[Authorize]
public class StatsModel : PageModel
{
    private readonly IStatsDashboardService _statsDashboardService;

    /// <summary>
    /// Initialise une nouvelle instance de la page des statistiques d'un biome.
    /// </summary>
    /// <param name="statsDashboardService">
    /// Service chargé de construire les données statistiques du biome.
    /// </param>
    public StatsModel(IStatsDashboardService statsDashboardService)
    {
        _statsDashboardService = statsDashboardService;
    }

    /// <summary>
    /// Slug du biome dont les statistiques doivent être affichées.
    /// Cette valeur provient de l'URL.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Période sélectionnée par l'utilisateur pour filtrer les statistiques.
    /// Exemple : 1, 7, 30, 365 ou all.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "30";

    /// <summary>
    /// Données complètes du tableau de bord statistique affichées dans la vue.
    /// </summary>
    public StatsDashboardVm Dashboard { get; private set; } =
        StatsDashboardVm.Empty("30", string.Empty, string.Empty);

    /// <summary>
    /// Traite la requête GET de la page statistiques.
    /// Récupère l'utilisateur connecté, charge les statistiques du biome demandé,
    /// puis renvoie la page ou une erreur adaptée.
    /// </summary>
    /// <returns>
    /// La page de statistiques si le biome existe,
    /// une demande de connexion si l'utilisateur n'est pas authentifié,
    /// ou une erreur 404 si le biome est introuvable.
    /// </returns>
    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Challenge();
        }

        var dashboard = await _statsDashboardService.BuildForBiomeAsync(userId, Slug, Period);

        if (dashboard is null)
        {
            return NotFound();
        }

        Dashboard = dashboard;
        Period = dashboard.Period;

        return Page();
    }

    /// <summary>
    /// Tente de récupérer l'identifiant de l'utilisateur connecté depuis ses claims.
    /// </summary>
    /// <param name="userId">
    /// Identifiant numérique de l'utilisateur si la récupération réussit.
    /// </param>
    /// <returns>
    /// True si l'identifiant utilisateur est présent et valide, sinon false.
    /// </returns>
    private bool TryGetCurrentUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out userId) && userId > 0;
    }
}