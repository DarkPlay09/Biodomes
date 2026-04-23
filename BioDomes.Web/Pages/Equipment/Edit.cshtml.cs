using System.Security.Claims;
using BioDomes.Domains.Entities;
using BioDomes.Domains.Enums;
using BioDomes.Domains.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

public class EditModel : PageModel
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentImageStorage _equipmentImageStorage;

    public EditModel(IEquipmentRepository equipmentRepository, IEquipmentImageStorage equipmentImageStorage)
    {
        _equipmentRepository = equipmentRepository;
        _equipmentImageStorage = equipmentImageStorage;
    }

    [BindProperty]
    public EquipmentInputModel Input { get; set; } = new();

    public IEnumerable<SelectListItem> ResourceOptions =>
        Enum.GetNames<ResourceType>()
            .Select(x => new SelectListItem(x, x));
    
    public string? CurrentImagePath { get; set; }

    public IActionResult OnGet(string slug)
    {
        var equipment = _equipmentRepository.GetBySlug(slug);
        if (equipment is null)
            return NotFound();

        Input.Name = equipment.Name;
        Input.ProducedElement = equipment.ProducedElement?.ToString();
        Input.ConsumedElement = equipment.ConsumedElement?.ToString();
        CurrentImagePath = equipment.ImagePath;

        return Page();
    }

    public async Task<IActionResult> OnPost(string slug)
    {
        var existingEquipment = _equipmentRepository.GetBySlug(slug);
        if (existingEquipment is null)
            return NotFound();

        CurrentImagePath = existingEquipment.ImagePath;

        if (!ModelState.IsValid)
            return Page();

        var produced = ParseResourceType(Input.ProducedElement);
        var consumed = ParseResourceType(Input.ConsumedElement);

        if (!ModelState.IsValid)
            return Page();

        if (produced is null && consumed is null)
        {
            ModelState.AddModelError(string.Empty, "Un equipement doit produire et/ou consommer au moins un element.");
            return Page();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
            return Challenge();

        var oldImagePath = existingEquipment.ImagePath;
        var imagePath = existingEquipment.ImagePath;
        var hasNewImage = false;

        if (Input.ImageFile is not null)
        {
            try
            {
                imagePath = await _equipmentImageStorage.SaveAsync(
                    Input.Name!,
                    Input.ImageFile.FileName,
                    Input.ImageFile.OpenReadStream());
                hasNewImage = true;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Input.ImageFile", ex.Message);
                return Page();
            }
        }

        if (hasNewImage)
        {
            _equipmentImageStorage.Delete(oldImagePath);
        }

        var equipment = new Domains.Entities.Equipment(
            Input.Name!,
            produced,
            consumed,
            imagePath,
            new UserAccount { Id = userId },
            existingEquipment.IsPublicAvailable)
        {
            Id = existingEquipment.Id
        };

        _equipmentRepository.Update(slug, equipment);

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
