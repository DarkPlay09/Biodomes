using System.ComponentModel.DataAnnotations;

namespace BioDomes.Web.Pages.Equipment;

public class EquipmentInputModel
{
    private const string AlphaSpacesDashes = @"^[A-Za-zÀ-ÖØ-öø-ÿ0-9\s-]+$";

    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le nom ne peut contenir que des lettres, chiffres, espaces ou tirets.")]
    [Display(Name = "Nom")]
    public string? Name { get; set; }

    [Display(Name = "Élément produit")]
    public string? ProducedElement { get; set; }

    [Display(Name = "Élément consommé")]
    public string? ConsumedElement { get; set; }

    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
}
