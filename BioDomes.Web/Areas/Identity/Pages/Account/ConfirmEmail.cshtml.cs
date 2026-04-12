using System.Text;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<ConfirmEmailModel> _logger;

    public ConfirmEmailModel(
        UserManager<UserEntity> userManager,
        ILogger<ConfirmEmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;

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