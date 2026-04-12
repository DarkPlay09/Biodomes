using System.Text;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Gère la confirmation de l'adresse e-mail d'un utilisateur via le lien reçu par e-mail.
/// </summary>
/// <remarks>
/// Cette page est appelée après le clic sur le lien de confirmation envoyé lors de l'inscription.
/// Le lien contient l'identifiant de l'utilisateur ainsi qu'un code de confirmation encodé.
/// </remarks>
public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<ConfirmEmailModel> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="ConfirmEmailModel" />.
    /// </summary>
    /// <param name="userManager">Service Identity permettant de retrouver et de confirmer un utilisateur.</param>
    /// <param name="logger">Service de journalisation pour tracer les erreurs éventuelles.</param>
    public ConfirmEmailModel(
        UserManager<UserEntity> userManager,
        ILogger<ConfirmEmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Indique si la confirmation de l'adresse e-mail a réussi.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Message affiché à l'utilisateur pour indiquer le résultat de l'opération.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Traite la confirmation d'e-mail à partir des paramètres présents dans l'URL.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur à confirmer.</param>
    /// <param name="code">Code de confirmation encodé transmis dans le lien d'e-mail.</param>
    /// <returns>
    /// La page de confirmation avec un état de succès ou d'échec.
    /// </returns>
    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            IsSuccess = false;
            Message = "Le lien de confirmation est invalide.";
            return Page();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            IsSuccess = false;
            Message = "Impossible de retrouver le compte associé à ce lien.";
            return Page();
        }

        string decodedCode;

        try
        {
            decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch
        {
            IsSuccess = false;
            Message = "Le code de confirmation est invalide.";
            return Page();
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

        if (result.Succeeded)
        {
            IsSuccess = true;
            Message = "Votre adresse e-mail a bien été confirmée. Vous pouvez maintenant vous connecter.";
            return Page();
        }

        foreach (var error in result.Errors)
        {
            _logger.LogWarning("Erreur confirmation email : {Code} - {Description}", error.Code, error.Description);
        }

        IsSuccess = false;
        Message = "Le lien de confirmation est invalide, expiré ou a déjà été utilisé.";
        return Page();
    }
}