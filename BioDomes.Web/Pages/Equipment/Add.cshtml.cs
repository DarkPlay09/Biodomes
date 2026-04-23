using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

public class AddModel : PageModel
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;

    public AddModel(IEquipmentRepository equipmentRepository, IEquipmentImageStorage equipmentImageStorage)
    {
        _equipmentRepository = equipmentRepository;
        _equipmentImageStorage = equipmentImageStorage;
    }

    [BindProperty]
    public EquipmentInputModel Input { get; set; } = new();

    public IEnumerable<SelectListItem> ResourceOptions =>
        Enum.GetNames<ResourceType>()
            .Select(x => new SelectListItem(x, x));

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Input.ImageFile is null)
        {
            ModelState.AddModelError("Input.ImageFile", "Une image est requise.");
            return Page();
        }

        var produced = ParseResourceType(Input.ProducedElement);
        var consumed = ParseResourceType(Input.ConsumedElement);

        if (!ModelState.IsValid)
            return Page();

        if (produced is null && consumed is null)
        {
            ModelState.AddModelError(string.Empty, "Un équipement doit produire et/ou consommer au moins un élément.");
            return Page();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            return Challenge();

        string? imagePath;
        try
        {
            imagePath = await _equipmentImageStorage.SaveAsync(
                Input.Name!,
                Input.ImageFile.FileName,
                Input.ImageFile.OpenReadStream());
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.ImageFile", ex.Message);
            return Page();
        }

        var equipment = new Domains.Entities.Equipment(
            Input.Name!,
            produced,
            consumed,
            imagePath,
            new UserAccount { Id = userId },
            isPublicAvailable: false
        );

        try
        {
            _equipmentRepository.Add(equipment);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.Name", ex.Message);
            return Page();
        }

        return RedirectToPage("/Equipment/Index");
    }

    private ResourceType? ParseResourceType(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        if (Enum.TryParse<ResourceType>(rawValue, out var parsed))
            return parsed;

        ModelState.AddModelError(string.Empty, $"Ressource invalide: {rawValue}.");
        return null;
    }
}
