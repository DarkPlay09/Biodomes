using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages.Biome;

public class AddModel : PageModel
{
    private readonly IBiomeRepository _biomeRepository;

    public AddModel(IBiomeRepository biomeRepository)
    {
        _biomeRepository = biomeRepository;
    }

    [BindProperty]
    public BiomeInputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string SafeReturnUrl =>
        Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("/Biome/Index")!;

    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl))
            return;

        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            ReturnUrl = refererUri.PathAndQuery;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (!TryGetCurrentUserId(out var currentUserId))
            return Challenge();

        try
        {
            var biome = new Domains.Entities.Biome(
                Input.Name!,
                Input.Temperature,
                Input.AbsoluteHumidity,
                new UserAccount { Id = currentUserId });

            _biomeRepository.Add(biome);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        if (Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("/Biome/Index");
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
