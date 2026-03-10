using System.ComponentModel.DataAnnotations;
using BioDomes.Web.Validators;

namespace BioDomes.Web.Pages.Species;

public class SpeciesInputModel
{
    private const string AlphaSpacesDashes = @"^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$";
    
    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le nom ne peut contenir que des lettres, espaces ou tirets.")]
    public string? Name { get; set ;}
    
    [Required(ErrorMessage = "Le type est requis.")]
    [IsSpeciesClassification(ErrorMessage = "Le type doit être une classification valide.")]
    public string? Type { get; set; }
    
    [Required(ErrorMessage = "Le régime alimentaire est requis.")]
    [RegularExpression(AlphaSpacesDashes, ErrorMessage = "Le régime alimentaire ne peut contenir que des lettres, espaces ou tirets.")]
    public string? Diet { get; set; }

    [Range(0, 10000, ErrorMessage = "La taille doit être comprise entre 0 et 10 000.")]
    public double AdultSize { get; set ;}
    
    [Url(ErrorMessage = "L'image doit être une URL valide.")]
    public string? ImageUrl { get; set; }
}