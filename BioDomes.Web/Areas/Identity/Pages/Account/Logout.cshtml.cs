using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<UserEntity> _signInManager;

    public LogoutModel(SignInManager<UserEntity> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult OnGet()
    {
        return RedirectToPage("/Index", new { area = "" });
    }

    public async Task<IActionResult> OnPost()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Index", new { area = "" });
    }
}
