using System.ComponentModel.DataAnnotations;
using BioDomes.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BioDomes.Web.Pages;

public class ContactModel : PageModel
{
    private readonly SmtpEmailSender _emailSender;

    public ContactModel(SmtpEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [BindProperty]
    public ContactInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _emailSender.SendContactEmailAsync(
                Input.FullName,
                Input.Email,
                Input.Subject,
                Input.Message);

            TempData["SuccessMessage"] = "Votre message a bien été envoyé. L’équipe BioDomes vous répondra prochainement.";

            return RedirectToPage("/Index");
        }
        catch
        {
            ModelState.AddModelError(
                string.Empty,
                "L’envoi du message a échoué. Veuillez réessayer plus tard.");

            return Page();
        }
    }

    public class ContactInputModel
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom complet ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Nom complet *")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L’adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L’adresse email n’est pas valide.")]
        [StringLength(150, ErrorMessage = "L’adresse email ne peut pas dépasser 150 caractères.")]
        [Display(Name = "Adresse email *")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le sujet est obligatoire.")]
        [StringLength(120, ErrorMessage = "Le sujet ne peut pas dépasser 120 caractères.")]
        [Display(Name = "Sujet *")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le message est obligatoire.")]
        [StringLength(1500, MinimumLength = 10, ErrorMessage = "Le message doit contenir entre 10 et 1500 caractères.")]
        [Display(Name = "Message *")]
        public string Message { get; set; } = string.Empty;
    }
}