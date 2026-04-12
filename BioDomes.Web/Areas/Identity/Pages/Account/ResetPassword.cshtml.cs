using System.ComponentModel.DataAnnotations;
using System.Text;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Gère la définition d’un nouveau mot de passe après une demande de réinitialisation.
/// </summary>
/// <remarks>
/// Cette page est accessible via le lien reçu par e-mail.
/// Le token transmis dans l’URL est décodé puis utilisé par Identity pour valider l’opération.
/// </remarks>
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="ResetPasswordModel" />.
    /// </summary>
    /// <param name="userManager">Service Identity utilisé pour retrouver l’utilisateur et réinitialiser son mot de passe.</param>
    public ResetPasswordModel(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Représente les données du formulaire de réinitialisation.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Représente les champs du formulaire de nouveau mot de passe.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// Adresse e-mail du compte concerné.
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nouveau mot de passe choisi par l’utilisateur.
        /// </summary>
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation du nouveau mot de passe.
        /// </summary>
        [Required(ErrorMessage = "La confirmation est requise.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmation")]
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Token de réinitialisation décodé et transmis à Identity.
        /// </summary>
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initialise la page de réinitialisation à partir du code et de l’e-mail reçus dans l’URL.
    /// </summary>
    /// <param name="code">Token de réinitialisation encodé.</param>
    /// <param name="email">Adresse e-mail du compte concerné.</param>
    /// <returns>
    /// Une redirection vers la page "Mot de passe oublié" si les paramètres sont invalides,
    /// sinon la page courante avec le modèle initialisé.
    /// </returns>
    public IActionResult OnGet(string? code = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("./ForgotPassword");
        }

        Input = new InputModel
        {
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
            Email = email
        };

        return Page();
    }

    /// <summary>
    /// Traite la demande de réinitialisation de mot de passe.
    /// </summary>
    /// <returns>
    /// Une redirection vers la page de confirmation en cas de succès,
    /// ou la page courante avec les erreurs éventuelles.
    /// </returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}