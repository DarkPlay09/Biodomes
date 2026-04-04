using System.ComponentModel.DataAnnotations;

namespace BioDomes.Web.Pages.Species;

public class SpeciesInputModel
{
    private const string AlphaSpacesDashes = @"^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$";

    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le nom ne peut contenir que des lettres, espaces ou tirets.")]
    [Display(Name = "Nom")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "La classification est requise.")]
    [Display(Name = "Classification")]
    public string? Classification { get; set; }

    [Required(ErrorMessage = "Le régime alimentaire est requis.")]
    [Display(Name = "Régime alimentaire")]
    public string? Diet { get; set; }

    [Required(ErrorMessage = "La Taille est requise.")]
    [Range(0.1, 10000, ErrorMessage = "La taille adulte doit être comprise entre 0.1 et 10 000.")]
    [Display(Name = "Taille adulte")]
    public double AdultSize { get; set; }

    [Required(ErrorMessage = "Le poids est requis.")]
    [Range(0.1, 100000000, ErrorMessage = "Le poids doit être positif.")]
    [Display(Name = "Poids")]
    public double Weight { get; set; }
    
    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
}