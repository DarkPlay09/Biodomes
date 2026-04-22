using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly UserManager<UserEntity> _userManager;

    public IndexModel(
        ISpeciesRepository speciesRepository,
        UserManager<UserEntity> userManager)
    {
        _speciesRepository = speciesRepository;
        _userManager = userManager;
    }

    public string DisplayName { get; private set; } = "Chercheur";
    public int SpeciesCount { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public string Role { get; private set; } = "User";

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is not null)
        {
            DisplayName = string.IsNullOrWhiteSpace(user.UserName) ? "Chercheur" : user.UserName;
            EmailConfirmed = user.EmailConfirmed;
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;
        }

        SpeciesCount = _speciesRepository.GetAll().Count;
    }
}