using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Page affichée lorsqu'un utilisateur tente d'accéder à une ressource interdite.
/// </summary>
public class AccessDeniedModel : PageModel
{
    /// <summary>
    /// Initialise la page d'accès refusé.
    /// </summary>
    public void OnGet()
    {
    }
}