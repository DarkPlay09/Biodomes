#nullable disable

using System.ComponentModel.DataAnnotations;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Gère l’authentification d’un utilisateur sur la plateforme BioDomes.
/// </summary>
/// <remarks>
/// Cette page permet la connexion via :
/// <list type="bullet">
/// <item><description>une adresse e-mail</description></item>
/// <item><description>ou un nom d’utilisateur</description></item>
/// </list>
/// puis vérifie le mot de passe avec ASP.NET Core Identity.
/// </remarks>
public class LoginModel : PageModel
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<LoginModel> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="LoginModel" />.
    /// </summary>
    /// <param name="signInManager">Service Identity chargé de l’authentification.</param>
    /// <param name="userManager">Service Identity chargé de retrouver les utilisateurs.</param>
    /// <param name="logger">Service de journalisation pour tracer les connexions et erreurs.</param>
    public LoginModel(
        SignInManager<UserEntity> signInManager,
        UserManager<UserEntity> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Représente les données saisies dans le formulaire de connexion.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Liste des fournisseurs d’authentification externes disponibles.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    /// <summary>
    /// URL de retour utilisée après une connexion réussie.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Message d’erreur temporaire à afficher à l’utilisateur.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Représente les champs du formulaire de connexion.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// Adresse e-mail ou nom d’utilisateur saisi par l’utilisateur.
        /// </summary>
        [Required(ErrorMessage = "L'email ou le nom d'utilisateur est requis.")]
        [Display(Name = "Email ou Nom d'utilisateur")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe associé au compte.
        /// </summary>
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Indique si la session doit être mémorisée sur l’appareil.
        /// </summary>
        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Initialise la page de connexion.
    /// </summary>
    /// <param name="returnUrl">URL de retour éventuelle après authentification.</param>
    /// <returns>Une tâche asynchrone représentant l’initialisation de la page.</returns>
    /// <remarks>
    /// Cette méthode :
    /// <list type="bullet">
    /// <item><description>affiche le message d’erreur temporaire si présent,</description></item>
    /// <item><description>vide le cookie externe existant,</description></item>
    /// <item><description>charge les providers d’authentification externes,</description></item>
    /// <item><description>initialise l’URL de retour.</description></item>
    /// </list>
    /// </remarks>
    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        ReturnUrl = returnUrl;
    }

    /// <summary>
    /// Traite la tentative de connexion.
    /// </summary>
    /// <param name="returnUrl">URL de retour après succès.</param>
    /// <returns>
    /// Une redirection vers la page appropriée en cas de succès ou de cas particuliers,
    /// sinon la page courante avec les erreurs de validation.
    /// </returns>
    /// <remarks>
    /// La méthode :
    /// <list type="number">
    /// <item><description>valide le formulaire,</description></item>
    /// <item><description>recherche l’utilisateur par e-mail ou par nom d’utilisateur,</description></item>
    /// <item><description>tente la connexion via <c>PasswordSignInAsync</c>,</description></item>
    /// <item><description>gère les cas de 2FA, verrouillage ou e-mail non confirmé.</description></item>
    /// </list>
    /// </remarks>
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string identifier = Input.Identifier.Trim();
        UserEntity? user = null;

        if (new EmailAddressAttribute().IsValid(identifier))
        {
            user = await _userManager.FindByEmailAsync(identifier);
        }

        user ??= await _userManager.FindByNameAsync(identifier);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Identifiants invalides.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("Utilisateur connecté.");
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new
            {
                ReturnUrl = returnUrl,
                RememberMe = Input.RememberMe
            });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Compte verrouillé.");
            return RedirectToPage("./Lockout");
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty,
                "Votre adresse e-mail n'est pas encore confirmée. Vérifiez votre boîte mail.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Identifiants invalides.");
        return Page();
    }
}