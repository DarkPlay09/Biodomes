using System.ComponentModel.DataAnnotations;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// Modèle représentant les champs du formulaire de création ou de modification d'un équipement.
/// Les attributs de validation permettent de contrôler les données avant la création de l'objet métier.
/// </summary>
public class EquipmentInputModel
{
    /// <summary>
    /// Expression régulière autorisant les lettres accentuées, chiffres, espaces et tirets.
    /// </summary>
    private const string AlphaSpacesDashes = @"^[A-Za-zÀ-ÖØ-öø-ÿ0-9\s-]+$";

    /// <summary>
    /// Nom de l'équipement.
    /// </summary>
    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le nom ne peut contenir que des lettres, chiffres, espaces ou tirets.")]
    [Display(Name = "Nom")]
    public string? Name { get; set; }

    /// <summary>
    /// Ressource produite par l'équipement.
    /// Ce champ peut être vide si l'équipement consomme au moins une ressource.
    /// </summary>
    [Display(Name = "Élément produit")]
    public string? ProducedElement { get; set; }

    /// <summary>
    /// Ressource consommée par l'équipement.
    /// Ce champ peut être vide si l'équipement produit au moins une ressource.
    /// </summary>
    [Display(Name = "Élément consommé")]
    public string? ConsumedElement { get; set; }

    /// <summary>
    /// Image envoyée depuis le formulaire.
    /// Elle est obligatoire lors de la création et optionnelle lors de la modification.
    /// </summary>
    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
}