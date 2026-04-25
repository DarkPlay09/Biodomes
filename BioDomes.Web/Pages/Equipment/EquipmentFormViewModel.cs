using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Equipment;

/// <summary>
/// ViewModel utilisé par le partial de formulaire des équipements.
/// Il regroupe les données saisies, les options de sélection et les informations
/// nécessaires à l'affichage de l'image actuelle lors d'une modification.
/// </summary>
public class EquipmentFormViewModel
{
    /// <summary>
    /// Données du formulaire d'équipement.
    /// </summary>
    public EquipmentInputModel Input { get; set; } = new();

    /// <summary>
    /// Options disponibles pour les ressources produites ou consommées.
    /// </summary>
    public IEnumerable<SelectListItem> ResourceOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    /// <summary>
    /// Chemin de l'image actuellement enregistrée pour l'équipement.
    /// </summary>
    public string? CurrentImagePath { get; set; }

    /// <summary>
    /// Nom du fichier image actuellement enregistré.
    /// </summary>
    public string? CurrentImageFileName { get; set; }
}