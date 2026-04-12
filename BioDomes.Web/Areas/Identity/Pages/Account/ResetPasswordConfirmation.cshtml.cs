using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Représente la page de confirmation affichée après une réinitialisation réussie du mot de passe.
/// </summary>
/// <remarks>
/// Cette classe ne contient pas de logique métier.
/// Elle sert uniquement de support à la vue de confirmation finale.
/// </remarks>
public class ResetPasswordConfirmationModel : PageModel
{
    /// <summary>
    /// Initialise la page de confirmation.
    /// </summary>
    public void OnGet()
    {
    }
}