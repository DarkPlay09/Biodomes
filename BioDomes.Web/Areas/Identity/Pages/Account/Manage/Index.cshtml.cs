using System.ComponentModel.DataAnnotations;
using BioDomes.Infrastructures.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    public string? Email { get; private set; }
    public string? CurrentAvatarPath { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "Date de naissance")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Organisation")]
        [StringLength(120, ErrorMessage = "L’organisation ne peut pas dépasser {1} caractères.")]
        public string? ResearchOrganization { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound("Utilisateur introuvable.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound("Utilisateur introuvable.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        if (Input.AvatarFile is not null)
        {
            var avatarPath = await SaveAvatarAsync(Input.AvatarFile);

            if (avatarPath is null)
            {
                await LoadAsync(user);
                return Page();
            }

            user.AvatarPath = avatarPath;
        }

        if (Input.BirthDate.HasValue)
        {
            user.BirthDate = DateOnly.FromDateTime(Input.BirthDate.Value);
        }

        user.ResearchOrganization = Input.ResearchOrganization?.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadAsync(user);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Ton profil a bien été mis à jour.";
        return RedirectToPage();
    }

    private async Task LoadAsync(UserEntity user)
    {
        Email = await _userManager.GetEmailAsync(user);
        CurrentAvatarPath = user.AvatarPath;

        Input = new InputModel
        {
            BirthDate = user.BirthDate == default
                ? null
                : user.BirthDate.ToDateTime(TimeOnly.MinValue),
            ResearchOrganization = user.ResearchOrganization
        };
    }

    private async Task<string?> SaveAvatarAsync(IFormFile file)
    {
        const long maxFileSize = 2 * 1024 * 1024;

        if (file.Length <= 0)
        {
            ModelState.AddModelError("Input.AvatarFile", "Le fichier est vide.");
            return null;
        }

        if (file.Length > maxFileSize)
        {
            ModelState.AddModelError("Input.AvatarFile", "L’image ne peut pas dépasser 2 Mo.");
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("Input.AvatarFile", "Le format de l’image doit être JPG, PNG ou WEBP.");
            return null;
        }

        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var uploadFolder = Path.Combine(webRootPath, "uploads", "avatars");

        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return $"/uploads/avatars/{fileName}";
    }
}