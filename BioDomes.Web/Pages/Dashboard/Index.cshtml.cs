using System.Globalization;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Dashboard;

/// <summary>
/// PageModel de la page principale du dashboard.
/// Cette page affiche un résumé personnalisé pour l'utilisateur connecté,
/// ainsi que quelques indicateurs globaux de la plateforme.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;

    /// <summary>
    /// Initialise le dashboard avec le service Identity permettant de récupérer
    /// les informations de l'utilisateur connecté.
    /// </summary>
    /// <param name="userManager">Service ASP.NET Identity de gestion des utilisateurs.</param>
    public IndexModel(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Nom affiché dans le message d'accueil du dashboard.
    /// </summary>
    public string DisplayName { get; private set; } = "Chercheur";

    /// <summary>
    /// Libellé du rôle affiché dans le dashboard.
    /// Pour le moment, cette valeur est statique.
    /// </summary>
    public string RoleLabel { get; private set; } = "Chercheur principal";

    /// <summary>
    /// Date actuelle formatée en français de Belgique.
    /// </summary>
    public string CurrentDateLabel { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre de biomes affiché dans les statistiques du dashboard.
    /// Valeur temporaire en attendant la connexion avec les repositories.
    /// </summary>
    public int BiomeCount { get; private set; } = 12;

    /// <summary>
    /// Nombre d'espèces affiché dans les statistiques du dashboard.
    /// Valeur temporaire en attendant la connexion avec les repositories.
    /// </summary>
    public int SpeciesCount { get; private set; } = 148;

    /// <summary>
    /// Nombre d'équipements affiché dans les statistiques du dashboard.
    /// Valeur temporaire en attendant la connexion avec les repositories.
    /// </summary>
    public int EquipmentCount { get; private set; } = 45;

    /// <summary>
    /// Pourcentage de biomes considérés comme équilibrés.
    /// Valeur temporaire en attendant un vrai calcul métier.
    /// </summary>
    public int BalancedBiomesPercent { get; private set; } = 85;

    /// <summary>
    /// Charge les données nécessaires à l'affichage du dashboard.
    /// La méthode récupère l'utilisateur connecté via Identity et prépare
    /// la date du jour dans un format lisible pour l'interface.
    /// </summary>
    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is not null)
        {
            DisplayName = string.IsNullOrWhiteSpace(user.UserName)
                ? "Chercheur"
                : user.UserName;
        }

        // Format belge francophone pour obtenir une date du type :
        // "samedi 25 avril 2026".
        var culture = new CultureInfo("fr-BE");
        CurrentDateLabel = DateTime.Now.ToString("dddd d MMMM yyyy", culture);

        // TODO plus tard :
        // remplacer les valeurs mockées par des vraies stats depuis les repositories.
        // BiomeCount = ...
        // SpeciesCount = ...
        // EquipmentCount = ...
        // BalancedBiomesPercent = ...
    }
}