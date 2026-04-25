using System.Globalization;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;

    public IndexModel(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public string DisplayName { get; private set; } = "Chercheur";
    public string RoleLabel { get; private set; } = "Chercheur principal";
    public string CurrentDateLabel { get; private set; } = string.Empty;

    public int BiomeCount { get; private set; } = 12;
    public int SpeciesCount { get; private set; } = 148;
    public int EquipmentCount { get; private set; } = 45;
    public int BalancedBiomesPercent { get; private set; } = 85;

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is not null)
        {
            DisplayName = string.IsNullOrWhiteSpace(user.UserName)
                ? "Chercheur"
                : user.UserName;
        }

        var culture = new CultureInfo("fr-BE");
        CurrentDateLabel = DateTime.Now.ToString("dddd d MMMM yyyy", culture);

        // TODO plus tard :
        // remplacer les valeurs mockées par des vraies stats depuis les repositories
        // BiomeCount = ...
        // SpeciesCount = ...
        // EquipmentCount = ...
        // BalancedBiomesPercent = ...
    }
}