using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// Modèle représentant les champs du formulaire de création ou de modification d'une espèce.
/// Les attributs de validation contrôlent les données avant leur transformation en objet métier.
/// </summary>
public class SpeciesInputModel
{
    /// <summary>
    /// Expression régulière autorisant les lettres accentuées, les espaces et les tirets.
    /// </summary>
    private const string AlphaSpacesDashes = @"^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$";

    /// <summary>
    /// Nom courant de l'espèce.
    /// </summary>
    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le nom ne peut contenir que des lettres, espaces ou tirets.")]
    [Display(Name = "Nom")]
    public string? Name { get; set; }

    /// <summary>
    /// Classification biologique de l'espèce.
    /// La valeur est reçue sous forme de texte puis convertie en enum dans le PageModel.
    /// </summary>
    [Required(ErrorMessage = "La classification est requise.")]
    [Display(Name = "Classification")]
    public string? Classification { get; set; }

    /// <summary>
    /// Régime alimentaire de l'espèce.
    /// La valeur est reçue sous forme de texte puis convertie en enum dans le PageModel.
    /// </summary>
    [Required(ErrorMessage = "Le régime alimentaire est requis.")]
    [Display(Name = "Régime alimentaire")]
    public string? Diet { get; set; }

    /// <summary>
    /// Taille adulte de l'espèce.
    /// </summary>
    [Required(ErrorMessage = "La Taille est requise.")]
    [Range(0, 10000, ErrorMessage = "La taille adulte doit être comprise entre 0.1 et 10 000.")]
    [Display(Name = "Taille adulte")]
    public double AdultSize { get; set; }

    /// <summary>
    /// Poids de l'espèce.
    /// </summary>
    [Required(ErrorMessage = "Le poids est requis.")]
    [Range(0, 100000000, ErrorMessage = "Le poids doit être positif.")]
    [Display(Name = "Poids")]
    public double Weight { get; set; }

    /// <summary>
    /// Image envoyée depuis le formulaire.
    /// Elle est obligatoire lors de la création et optionnelle lors de la modification.
    /// </summary>
    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
    
    public bool CanManage { get; set; }

    public int ImpactedBiomesCount { get; set; }
}