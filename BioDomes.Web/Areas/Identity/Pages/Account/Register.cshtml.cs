using System.ComponentModel.DataAnnotations;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 30 caractères.")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Le nom d'utilisateur ne peut contenir que des lettres, chiffres, points, tirets et underscores.")]
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de naissance est requise.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateOnly BirthDate { get; set; }

        [StringLength(100, ErrorMessage = "Le nom de l'organisation ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Organisation de recherche")]
        public string? ResearchOrganization { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

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
            Role = "User"
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Nouvel utilisateur créé.");

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}