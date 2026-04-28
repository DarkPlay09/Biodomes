using System.ComponentModel.DataAnnotations;

namespace BioDomes.Web.Pages.Biome;

public class BiomeInputModel
{
    private const string NamePattern = @"^[A-Za-zÀ-ÖØ-öø-ÿ0-9\s-]+$";

    [Required(ErrorMessage = "Le nom est requis.")]
    [RegularExpression(NamePattern, ErrorMessage = "Le nom ne peut contenir que des lettres, chiffres, espaces ou tirets.")]
    [Display(Name = "Nom")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "La température est requise.")]
    [Range(-100, 200, ErrorMessage = "La température doit être comprise entre -100 et 200°C.")]
    [Display(Name = "Température (°C)")]
    public double Temperature { get; set; }

    [Required(ErrorMessage = "L'humidité absolue est requise.")]
    [Range(0, 1000, ErrorMessage = "L'humidité absolue doit être comprise entre 0 et 1000 g/m³.")]
    [Display(Name = "Humidité absolue (g/m³)")]
    public double AbsoluteHumidity { get; set; }
}
