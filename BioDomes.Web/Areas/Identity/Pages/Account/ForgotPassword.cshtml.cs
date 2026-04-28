using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Gère la demande de réinitialisation de mot de passe.
/// </summary>
/// <remarks>
/// Cette page permet à l'utilisateur de saisir son adresse e-mail afin de recevoir
/// un lien sécurisé de réinitialisation de mot de passe.
/// </remarks>
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="ForgotPasswordModel" />.
    /// </summary>
    /// <param name="userManager">Service Identity permettant de retrouver les utilisateurs et générer un token de réinitialisation.</param>
    /// <param name="emailSender">Service chargé d'envoyer l'e-mail de réinitialisation.</param>
    /// <param name="logger">Service de journalisation utilisé pour tracer l'envoi des e-mails.</param>
    public ForgotPasswordModel(
        UserManager<UserEntity> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>
    /// Données saisies dans le formulaire.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// URL de retour éventuelle après l'opération.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Représente les données du formulaire "Mot de passe oublié".
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// Adresse e-mail du compte pour lequel une réinitialisation est demandée.
        /// </summary>
        [Required(ErrorMessage = "L'adresse e-mail est requise.")]
        [EmailAddress(ErrorMessage = "Format d'e-mail invalide.")]
        [Display(Name = "Email professionnel")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initialise la page et récupère l'URL de retour éventuelle.
    /// </summary>
    /// <param name="returnUrl">URL de retour à conserver dans le flux.</param>
    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(Url.Content("~/Dashboard"));
        }
        
        ReturnUrl = returnUrl;

        return Page();
    }

    /// <summary>
    /// Traite la demande de réinitialisation de mot de passe.
    /// </summary>
    /// <param name="returnUrl">URL de retour à conserver après redirection.</param>
    /// <returns>
    /// La même page si le modèle est invalide, sinon une redirection vers la page de confirmation.
    /// </returns>
    /// <remarks>
    /// Pour des raisons de sécurité, la redirection vers la page de confirmation a lieu même
    /// si l'utilisateur n'existe pas ou si son e-mail n'est pas confirmé.
    /// </remarks>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(Url.Content("~/Dashboard"));
        }
        
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string email = Input.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email },
                protocol: Request.Scheme);

            if (callbackUrl is not null)
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Réinitialisez votre mot de passe BioDomes",
                    BuildResetPasswordEmail(user.UserName ?? email, callbackUrl));

                _logger.LogInformation("Email de réinitialisation envoyé à {Email}.", email);
            }
        }

        return RedirectToPage("./ForgotPasswordConfirmation", new { email, returnUrl });
    }

    /// <summary>
    /// Construit le contenu HTML de l'e-mail de réinitialisation de mot de passe.
    /// </summary>
    /// <param name="userName">Nom d'utilisateur ou identifiant affiché dans l'e-mail.</param>
    /// <param name="callbackUrl">Lien sécurisé menant à la page de réinitialisation.</param>
    /// <returns>Le contenu HTML complet de l'e-mail.</returns>
    private static string BuildResetPasswordEmail(string userName, string callbackUrl)
    {
        string safeName = HtmlEncoder.Default.Encode(userName);
        string safeUrl = HtmlEncoder.Default.Encode(callbackUrl);

        return $"""
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <title>Réinitialisation BioDomes</title>
</head>
<body style="margin:0;padding:0;background-color:#edf2f1;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
    <div style="padding:32px 16px;">
        <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 18px 40px rgba(15,23,42,0.08);">
            <div style="background:linear-gradient(135deg,#0c4f49,#0a3d38);padding:28px 32px;color:#ffffff;">
                <div style="font-size:28px;font-weight:800;letter-spacing:-0.02em;">BioDomes</div>
                <div style="margin-top:8px;font-size:15px;opacity:0.9;">Réinitialisation de votre mot de passe</div>
            </div>

            <div style="padding:32px;">
                <h1 style="margin:0 0 16px;font-size:30px;line-height:1.1;color:#101828;">
                    Mot de passe oublié
                </h1>

                <p style="margin:0 0 14px;font-size:16px;line-height:1.7;color:#475467;">
                    Bonjour {safeName},
                </p>

                <p style="margin:0 0 18px;font-size:16px;line-height:1.7;color:#475467;">
                    Nous avons reçu une demande de réinitialisation de votre mot de passe BioDomes.
                    Cliquez sur le bouton ci-dessous pour choisir un nouveau mot de passe.
                </p>

                <div style="margin:28px 0;text-align:center;">
                    <a href="{safeUrl}"
                       style="display:inline-block;padding:14px 28px;border-radius:14px;background:#0c4f49;color:#ffffff;text-decoration:none;font-weight:700;font-size:16px;">
                        Réinitialiser mon mot de passe
                    </a>
                </div>

                <p style="margin:0 0 10px;font-size:14px;line-height:1.7;color:#667085;">
                    Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet e-mail.
                </p>

                <p style="margin:0 0 10px;font-size:14px;line-height:1.7;color:#667085;">
                    Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur :
                </p>

                <p style="margin:0 0 22px;word-break:break-word;font-size:13px;line-height:1.7;color:#0c4f49;">
                    {safeUrl}
                </p>

                <div style="margin-top:24px;padding-top:20px;border-top:1px solid #e4e7ec;font-size:13px;line-height:1.7;color:#98a2b3;">
                    BioDomes Research Initiative · Solutions durables pour un monde en évolution.
                </div>
            </div>
        </div>
    </div>
</body>
</html>
""";
    }
}