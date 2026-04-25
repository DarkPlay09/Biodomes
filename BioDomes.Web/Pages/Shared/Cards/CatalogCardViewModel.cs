namespace BioDomes.Web.Pages.Shared.Cards;

/// <summary>
/// ViewModel générique représentant une carte de catalogue.
/// Ce modèle est utilisé pour afficher différents types d'entités
/// sous forme de cartes réutilisables, par exemple des équipements,
/// des espèces ou d'autres éléments du domaine.
/// </summary>
public class CatalogCardViewModel
{
    /// <summary>
    /// Titre principal affiché sur la carte.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Chemin de l'image affichée dans la carte.
    /// Peut être null si aucune image n'est disponible.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Badge optionnel affiché sur la carte.
    /// Peut servir à indiquer un statut, une visibilité ou une catégorie.
    /// </summary>
    public string? Badge { get; set; }

    /// <summary>
    /// Liste des informations secondaires affichées dans la carte.
    /// Chaque élément contient un libellé et une valeur.
    /// </summary>
    public IReadOnlyList<CatalogCardMetaItem> Meta { get; set; } = [];

    /// <summary>
    /// Page Razor vers laquelle rediriger pour modifier l'entité.
    /// Null si l'utilisateur ne peut pas modifier l'élément.
    /// </summary>
    public string? EditPage { get; set; }

    /// <summary>
    /// Paramètres de route nécessaires pour accéder à la page de modification.
    /// Par exemple : slug, id, ou autre identifiant.
    /// </summary>
    public IDictionary<string, string>? EditRouteValues { get; set; }

    /// <summary>
    /// Page Razor utilisée pour envoyer la requête de suppression.
    /// Null si l'utilisateur ne peut pas supprimer l'élément.
    /// </summary>
    public string? DeletePage { get; set; }

    /// <summary>
    /// Paramètres de route nécessaires pour identifier l'élément à supprimer.
    /// </summary>
    public IDictionary<string, string>? DeleteRouteValues { get; set; }
}

/// <summary>
/// Représente une ligne d'information affichée dans une carte de catalogue.
/// Exemple : "Produit : Oxygène" ou "Consomme : Eau".
/// </summary>
public class CatalogCardMetaItem
{
    /// <summary>
    /// Libellé de l'information affichée.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Valeur associée au libellé.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}