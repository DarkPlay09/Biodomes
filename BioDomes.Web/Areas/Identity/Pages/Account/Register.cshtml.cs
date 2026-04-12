using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Gère l’inscription d’un nouvel utilisateur sur BioDomes.
/// </summary>
/// <remarks>
/// Cette page :
/// <list type="bullet">
/// <item><description>valide les champs du formulaire,</description></item>
/// <item><description>crée le compte utilisateur,</description></item>
/// <item><description>génère un token de confirmation d’e-mail,</description></item>
/// <item><description>envoie un e-mail de confirmation,</description></item>
/// <item><description>redirige vers la page de confirmation d’inscription.</description></item>
/// </list>
/// </remarks>
public class RegisterModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterModel> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="RegisterModel" />.
    /// </summary>
    /// <param name="userManager">Service Identity chargé de créer et retrouver les utilisateurs.</param>
    /// <param name="signInManager">Service Identity chargé des providers externes éventuels.</param>
    /// <param name="emailSender">Service chargé d’envoyer l’e-mail de confirmation.</param>
    /// <param name="logger">Service de journalisation.</param>
    public RegisterModel(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IEmailSender emailSender,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>
    /// Représente les données saisies dans le formulaire d’inscription.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Liste des fournisseurs d’authentification externes disponibles.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    /// <summary>
    /// URL de retour éventuelle après l’inscription.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Représente les champs du formulaire d’inscription.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// Nom d’utilisateur unique choisi par l’utilisateur.
        /// </summary>
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 30 caractères.")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres, points, tirets et underscores.")]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Adresse e-mail professionnelle de l’utilisateur.
        /// </summary>
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [Display(Name = "Email professionnel")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Date de naissance renseignée à l’inscription.
        /// </summary>
        [Required(ErrorMessage = "La date de naissance est requise.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// Organisation de recherche de l’utilisateur.
        /// </summary>
        [StringLength(100, ErrorMessage = "Le nom de l'organisation ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Organisation de recherche")]
        public string? ResearchOrganization { get; set; }

        /// <summary>
        /// Mot de passe du compte.
        /// </summary>
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation du mot de passe.
        /// </summary>
        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirmation")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initialise la page d’inscription.
    /// </summary>
    /// <param name="returnUrl">URL de retour éventuelle après l’inscription.</param>
    /// <returns>Une tâche asynchrone représentant l’initialisation de la page.</returns>
    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    /// <summary>
    /// Traite la demande d’inscription d’un nouvel utilisateur.
    /// </summary>
    /// <param name="returnUrl">URL de retour éventuelle.</param>
    /// <returns>
    /// La page courante si des erreurs sont détectées, sinon une redirection vers
    /// la page de confirmation d’inscription.
    /// </returns>
    /// <remarks>
    /// Cette méthode vérifie d’abord l’unicité du nom d’utilisateur et de l’e-mail,
    /// puis crée l’utilisateur et envoie un e-mail de confirmation.
    /// </remarks>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string userName = Input.UserName.Trim();
        string email = Input.Email.Trim();

        var existingUserByName = await _userManager.FindByNameAsync(userName);
        if (existingUserByName is not null)
        {
            ModelState.AddModelError("Input.UserName", "Ce nom d'utilisateur est déjà utilisé.");
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(email);
        if (existingUserByEmail is not null)
        {
            ModelState.AddModelError("Input.Email", "Cet email est déjà utilisé.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new UserEntity
        {
            UserName = userName,
            Email = email,
            BirthDate = Input.BirthDate,
            ResearchOrganization = string.IsNullOrWhiteSpace(Input.ResearchOrganization)
                ? null
                : Input.ResearchOrganization.Trim(),
            Role = "User",
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        _logger.LogInformation("Nouvel utilisateur créé.");

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code, returnUrl },
            protocol: Request.Scheme);

        if (callbackUrl is null)
        {
            ModelState.AddModelError(string.Empty, "Impossible de générer le lien de confirmation.");
            return Page();
        }

        await _emailSender.SendEmailAsync(
            email,
            "Confirmez votre compte BioDomes",
            BuildConfirmationEmail(userName, callbackUrl));

        return RedirectToPage("./RegisterConfirmation", new { email, returnUrl });
    }

    /// <summary>
    /// Construit le contenu HTML de l’e-mail de confirmation d’inscription.
    /// </summary>
    /// <param name="userName">Nom de l’utilisateur affiché dans l’e-mail.</param>
    /// <param name="callbackUrl">Lien sécurisé de confirmation d’e-mail.</param>
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