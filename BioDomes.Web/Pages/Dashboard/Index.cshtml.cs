using System.Globalization;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Web.Pages.Dashboard;

/// <summary>
/// PageModel de la page principale du dashboard.
/// Cette page affiche un résumé personnalisé pour l'utilisateur connecté,
/// ainsi que quelques indicateurs réels issus de la base de données.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly BioDomesDbContext _context;
    private readonly ISlugService _slugService;

    public IndexModel(
        UserManager<UserEntity> userManager,
        BioDomesDbContext context,
        ISlugService slugService)
    {
        _userManager = userManager;
        _context = context;
        _slugService = slugService;
    }

    public string DisplayName { get; private set; } = "Chercheur";

    public string RoleLabel { get; private set; } = "Chercheur principal";

    public string CurrentDateLabel { get; private set; } = string.Empty;

    public int BiomeCount { get; private set; }

    public int SpeciesCount { get; private set; }

    public int EquipmentCount { get; private set; }

    public int BalancedBiomesPercent { get; private set; }

    public int OptimalBiomeCount { get; private set; }

    public int UnstableBiomeCount { get; private set; }

    public int CriticalBiomeCount { get; private set; }

    public int AlertCount => Alerts.Count;

    public string? FirstBiomeSlug => RecentBiomes.FirstOrDefault()?.Slug;

    public IReadOnlyList<RecentBiomeCardViewModel> RecentBiomes { get; private set; }
        = new List<RecentBiomeCardViewModel>();

    public IReadOnlyList<DashboardAlertViewModel> Alerts { get; private set; }
        = new List<DashboardAlertViewModel>();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is not null)
        {
            DisplayName = string.IsNullOrWhiteSpace(user.UserName)
                ? "Chercheur"
                : user.UserName;

            RoleLabel = string.IsNullOrWhiteSpace(user.Role)
                ? "Chercheur principal"
                : user.Role;
        }

        var culture = new CultureInfo("fr-BE");
        CurrentDateLabel = DateTime.Now.ToString("dddd d MMMM yyyy", culture);

        if (user is null)
        {
            return;
        }

        var userId = user.Id;

        var userBiomesQuery = _context.Biomes
            .AsNoTracking()
            .Where(biome => biome.CreatorId == userId);

        BiomeCount = await userBiomesQuery.CountAsync();

        SpeciesCount = await _context.Species
            .AsNoTracking()
            .CountAsync(species => species.CreatorId == userId);

        EquipmentCount = await _context.Equipments
            .AsNoTracking()
            .CountAsync(equipment => equipment.CreatorId == userId);

        OptimalBiomeCount = await userBiomesQuery
            .CountAsync(biome => biome.State == "Optimal");

        UnstableBiomeCount = await userBiomesQuery
            .CountAsync(biome => biome.State == "Instable");

        CriticalBiomeCount = await userBiomesQuery
            .CountAsync(biome => biome.State == "Critique");

        BalancedBiomesPercent = BiomeCount == 0
            ? 0
            : (int)Math.Round((double)OptimalBiomeCount / BiomeCount * 100);

        var recentBiomeRows = await userBiomesQuery
            .OrderByDescending(biome => biome.UpdatedAt)
            .Take(3)
            .Select(biome => new RecentBiomeRow(
                biome.Name,
                biome.State,
                biome.Temperature,
                biome.AbsoluteHumidity,
                biome.BiomeSpeciesLinks.Count(),
                biome.BiomeEquipmentLinks.Count(),
                biome.UpdatedAt))
            .ToListAsync();

        RecentBiomes = recentBiomeRows
            .Select((biome, index) => ToRecentBiomeCardViewModel(biome, index, culture))
            .ToList();

        var alertBiomeRows = await userBiomesQuery
            .Where(biome => biome.State == "Critique" || biome.State == "Instable")
            .OrderBy(biome => biome.State == "Critique" ? 0 : 1)
            .ThenByDescending(biome => biome.UpdatedAt)
            .Select(biome => new AlertBiomeRow(
                biome.Name,
                biome.State))
            .ToListAsync();

        Alerts = BuildAlerts(alertBiomeRows);
    }

    private RecentBiomeCardViewModel ToRecentBiomeCardViewModel(
        RecentBiomeRow biome,
        int index,
        CultureInfo culture)
    {
        var speciesCount = biome.SpeciesCount;
        var equipmentCount = biome.EquipmentCount;

        return new RecentBiomeCardViewModel
        {
            Name = biome.Name,
            Slug = _slugService.ToSlug(biome.Name),
            StateLabel = FormatState(biome.State),
            StateCssClass = GetStateCssClass(biome.State),
            TemperatureLabel = $"{biome.Temperature.ToString("0.#", culture)}°C",
            AbsoluteHumidityLabel = $"{biome.AbsoluteHumidity.ToString("0.#", culture)} g/m³",
            SpeciesCount = speciesCount,
            EquipmentCount = equipmentCount,
            StabilityScore = CalculateStabilityScore(biome.State, speciesCount, equipmentCount),
            TemperatureCssClass = IsTemperatureWarning(biome.State) ? "dashboard-metric-warning" : string.Empty
        };
    }

    private static List<DashboardAlertViewModel> BuildAlerts(
        IReadOnlyList<AlertBiomeRow> alertBiomeRows)
    {
        var alerts = new List<DashboardAlertViewModel>();

        foreach (var biome in alertBiomeRows)
        {
            if (biome.State == "Critique")
            {
                alerts.Add(new DashboardAlertViewModel
                {
                    Title = $"{biome.Name} est en état critique",
                    Message = "Ce biome nécessite une intervention prioritaire. Vérifiez sa température, son humidité, ses espèces et ses équipements.",
                    CssClass = "dashboard-alert--danger"
                });
            }
            else if (biome.State == "Instable")
            {
                alerts.Add(new DashboardAlertViewModel
                {
                    Title = $"{biome.Name} est instable",
                    Message = "Ce biome présente un équilibre fragile. Une vérification est recommandée.",
                    CssClass = "dashboard-alert--warning"
                });
            }
        }

        return alerts;
    }

    private static string FormatState(string state)
    {
        return state switch
        {
            "Optimal" => "Optimal",
            "Instable" => "Instable",
            "Critique" => "Critique",
            _ => "Inconnu"
        };
    }

    private static string GetStateCssClass(string state)
    {
        return state switch
        {
            "Optimal" => "dashboard-status-pill--success",
            "Instable" => "dashboard-status-pill--warning",
            "Critique" => "dashboard-status-pill--danger",
            _ => "dashboard-status-pill--neutral"
        };
    }

    private static bool IsTemperatureWarning(string state)
    {
        return state is "Instable" or "Critique";
    }

    private static int CalculateStabilityScore(string state, int speciesCount, int equipmentCount)
    {
        var baseScore = state switch
        {
            "Optimal" => 85,
            "Instable" => 55,
            "Critique" => 20,
            _ => 40
        };

        var speciesBonus = Math.Min(speciesCount * 2, 10);
        var equipmentBonus = Math.Min(equipmentCount * 2, 5);

        return Math.Clamp(baseScore + speciesBonus + equipmentBonus, 0, 100);
    }

    private sealed record RecentBiomeRow(
        string Name,
        string State,
        double Temperature,
        double AbsoluteHumidity,
        int SpeciesCount,
        int EquipmentCount,
        DateTime UpdatedAt);
    
    private sealed record AlertBiomeRow(
        string Name,
        string State);
}

public class RecentBiomeCardViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string StateLabel { get; init; } = string.Empty;

    public string StateCssClass { get; init; } = string.Empty;

    public string TemperatureLabel { get; init; } = string.Empty;

    public string AbsoluteHumidityLabel { get; init; } = string.Empty;

    public string TemperatureCssClass { get; init; } = string.Empty;

    public int SpeciesCount { get; init; }

    public int EquipmentCount { get; init; }

    public int StabilityScore { get; init; }
}

public class DashboardAlertViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string CssClass { get; init; } = string.Empty;
}