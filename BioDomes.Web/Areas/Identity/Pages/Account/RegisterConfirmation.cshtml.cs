using System.Text;
using System.Text.Encodings.Web;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Affiche la confirmation d’inscription et permet de renvoyer l’e-mail de confirmation.
/// </summary>
/// <remarks>
/// Cette page applique un cooldown côté serveur afin de limiter le renvoi abusif des e-mails.
/// </remarks>
public class RegisterConfirmationModel : PageModel
{
    /// <summary>
    /// Durée du cooldown, en secondes, avant d’autoriser un nouvel envoi.
    /// </summary>
    private const int ResendCooldownSeconds = 60;

    private readonly UserManager<UserEntity> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RegisterConfirmationModel> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="RegisterConfirmationModel" />.
    /// </summary>
    /// <param name="userManager">Service Identity utilisé pour retrouver le compte et générer le token de confirmation.</param>
    /// <param name="emailSender">Service chargé d’envoyer les e-mails de confirmation.</param>
    /// <param name="memoryCache">Cache mémoire utilisé pour stocker le cooldown.</param>
    /// <param name="logger">Service de journalisation.</param>
    public RegisterConfirmationModel(
        UserManager<UserEntity> userManager,
        IEmailSender emailSender,
        IMemoryCache memoryCache,
        ILogger<RegisterConfirmationModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Adresse e-mail du compte en attente de confirmation.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL de retour éventuelle à conserver dans le flux.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Message d’état affiché après une tentative de renvoi.
    /// </summary>
    public string? StatusMessage { get; private set; }

    /// <summary>
    /// Indique si l’action précédente est considérée comme réussie.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Nombre de secondes restantes avant autorisation d’un nouvel envoi.
    /// </summary>
    public int CooldownRemainingSeconds { get; private set; }

    /// <summary>
    /// Initialise la page et calcule le cooldown restant.
    /// </summary>
    public void OnGet()
    {
        CooldownRemainingSeconds = GetRemainingCooldownSeconds(Email);
    }

    /// <summary>
    /// Tente de renvoyer un e-mail de confirmation d’inscription.
    /// </summary>
    /// <returns>
    /// La même page avec le message de statut et le cooldown mis à jour.
    /// </returns>
    public async Task<IActionResult> OnPostResendAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            StatusMessage = "Adresse e-mail introuvable.";
            IsSuccess = false;
            return Page();
        }

        CooldownRemainingSeconds = GetRemainingCooldownSeconds(Email);
        if (CooldownRemainingSeconds > 0)
        {
            StatusMessage = $"Veuillez attendre {CooldownRemainingSeconds} seconde(s) avant un nouvel envoi.";
            IsSuccess = false;
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user is null)
        {
            StatusMessage = "Impossible de retrouver le compte associé à cette adresse.";
            IsSuccess = false;
            return Page();
        }

        if (user.EmailConfirmed)
        {
            StatusMessage = "Cette adresse e-mail est déjà confirmée.";
            IsSuccess = true;
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code, returnUrl = ReturnUrl },
            protocol: Request.Scheme);

        if (callbackUrl is null)
        {
            StatusMessage = "Impossible de générer le lien de confirmation.";
            IsSuccess = false;
            return Page();
        }

        await _emailSender.SendEmailAsync(
            Email,
            "Confirmez votre compte BioDomes",
            BuildConfirmationEmail(user.UserName ?? Email, callbackUrl));

        SetCooldown(Email);

        CooldownRemainingSeconds = ResendCooldownSeconds;
        StatusMessage = "Un nouvel e-mail de confirmation vient d’être envoyé.";
        IsSuccess = true;

        _logger.LogInformation("E-mail de confirmation renvoyé à {Email}.", Email);

        return Page();
    }

    /// <summary>
    /// Calcule le nombre de secondes restantes avant un nouvel envoi autorisé.
    /// </summary>
    /// <param name="email">Adresse e-mail utilisée comme identifiant du cooldown.</param>
    /// <returns>Le nombre de secondes restantes, ou 0 si aucun cooldown n’est actif.</returns>
    private int GetRemainingCooldownSeconds(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return 0;
        }

        if (_memoryCache.TryGetValue<DateTimeOffset>(GetCooldownKey(email), out var expiresAt))
        {
            var remaining = (int)Math.Ceiling((expiresAt - DateTimeOffset.UtcNow).TotalSeconds);
            return Math.Max(0, remaining);
        }

        return 0;
    }

    /// <summary>
    /// Enregistre un cooldown de renvoi pour l’adresse e-mail donnée.
    /// </summary>
    /// <param name="email">Adresse e-mail concernée.</param>
    private void SetCooldown(string email)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(ResendCooldownSeconds);

        _memoryCache.Set(
            GetCooldownKey(email),
            expiresAt,
            expiresAt);
    }

    /// <summary>
    /// Construit la clé de cache utilisée pour stocker le cooldown.
    /// </summary>
    /// <param name="email">Adresse e-mail servant d’identifiant logique.</param>
    /// <returns>Une clé normalisée et unique.</returns>
    private static string GetCooldownKey(string email)
        => $"register-confirmation-resend:{email.Trim().ToLowerInvariant()}";

    /// <summary>
    /// Construit le contenu HTML de l’e-mail de confirmation d’inscription.
    /// </summary>
    /// <param name="userName">Nom affiché dans l’e-mail.</param>
    /// <param name="callbackUrl">Lien sécurisé de confirmation.</param>
    /// <returns>Le contenu HTML complet de l’e-mail.</returns>
    private static string BuildConfirmationEmail(string userName, string callbackUrl)
    {
        string safeName = HtmlEncoder.Default.Encode(userName);
        string safeUrl = HtmlEncoder.Default.Encode(callbackUrl);

        return $"""
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <title>Confirmation BioDomes</title>
</head>
<body style="margin:0;padding:0;background-color:#edf2f1;font-family:Arial,Helvetica,sans-serif;color:#0f172a;">
    <div style="padding:32px 16px;">
        <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 18px 40px rgba(15,23,42,0.08);">
            <div style="background:linear-gradient(135deg,#0c4f49,#0a3d38);padding:28px 32px;color:#ffffff;">
                <div style="font-size:28px;font-weight:800;letter-spacing:-0.02em;">BioDomes</div>
                <div style="margin-top:8px;font-size:15px;opacity:0.9;">Confirmation de votre inscription</div>
            </div>

            <div style="padding:32px;">
                <h1 style="margin:0 0 16px;font-size:30px;line-height:1.1;color:#101828;">
                    Bienvenue sur BioDomes
                </h1>

                <p style="margin:0 0 14px;font-size:16px;line-height:1.7;color:#475467;">
                    Bonjour {safeName},
                </p>

                <p style="margin:0 0 18px;font-size:16px;line-height:1.7;color:#475467;">
                    Votre inscription a bien été enregistrée. Pour activer votre compte,
                    confirmez votre adresse e-mail en cliquant sur le bouton ci-dessous.
                </p>

                <div style="margin:28px 0;text-align:center;">
                    <a href="{safeUrl}"
                       style="display:inline-block;padding:14px 28px;border-radius:14px;background:#0c4f49;color:#ffffff;text-decoration:none;font-weight:700;font-size:16px;">
                        Confirmer mon e-mail
                    </a>
                </div>

                <p style="margin:0 0 10px;font-size:14px;line-height:1.7;color:#667085;">
                    Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur :
                </p>

                <p style="margin:0 0 22px;word-break:break-word;font-size:13px;line-height:1.7;color:#0c4f49;">
                    {safeUrl}
                </p>

                <div style="margin-top:24px;padding-top:20px;border-top:1px solid #e4e7ec;font-size:13px;line-height:1.7;color:#98a2b3;">
                    © 2026 BioDomes - Terrarium Zero. Tous droits réservés.
                </div>
            </div>
        </div>
    </div>
</body>
</html>
""";
    }
}