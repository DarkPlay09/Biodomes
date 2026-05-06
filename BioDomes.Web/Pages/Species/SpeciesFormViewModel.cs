using Microsoft.AspNetCore.Mvc.Rendering;

namespace BioDomes.Web.Pages.Species;

/// <summary>
/// ViewModel utilisé par le partial de formulaire des espèces.
/// Il regroupe les données saisies, les options des listes déroulantes
/// et les informations nécessaires à l'affichage de l'image actuelle.
/// </summary>
public class SpeciesFormViewModel
{
    /// <summary>
    /// Données du formulaire de création ou de modification.
    /// </summary>
    public SpeciesInputModel Input { get; set; } = new();

    /// <summary>
    /// Options disponibles pour la classification.
    /// </summary>
    public IEnumerable<SelectListItem> ClassificationOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    /// <summary>
    /// Options disponibles pour le régime alimentaire.
    /// </summary>
    public IEnumerable<SelectListItem> DietOptions { get; set; } = Enumerable.Empty<SelectListItem>();

    /// <summary>
    /// Chemin de l'image actuellement enregistrée.
    /// Utilisé principalement lors de la modification.
    /// </summary>
    public string? CurrentImagePath { get; set; }

    /// <summary>
    /// Nom du fichier image actuellement enregistré.
    /// Utilisé pour l'affichage dans le champ de téléversement.
    /// </summary>
    public string? CurrentImageFileName { get; set; }
}