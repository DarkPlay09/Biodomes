namespace BioDomes.Web.ViewModels.Stats;

/// <summary>
/// ViewModel principal utilisé par la page de statistiques d'un biome.
/// Il contient les informations du biome, les KPI, les données des graphiques
/// et les espèces affichées dans le tableau.
/// </summary>
public sealed class StatsDashboardVm
{
    /// <summary>
    /// Période actuellement sélectionnée dans le filtre.
    /// </summary>
    public string Period { get; init; } = "30";

    /// <summary>
    /// Indique si le biome possède suffisamment de données pour afficher les statistiques.
    /// </summary>
    public bool HasData { get; init; }

    /// <summary>
    /// Nom du biome analysé.
    /// </summary>
    public string BiomeName { get; init; } = string.Empty;

    /// <summary>
    /// Slug du biome utilisé dans les liens de navigation.
    /// </summary>
    public string BiomeSlug { get; init; } = string.Empty;

    /// <summary>
    /// État actuel du biome.
    /// Exemple : Optimal, Instable ou Critique.
    /// </summary>
    public string BiomeState { get; init; } = string.Empty;

    /// <summary>
    /// Température actuelle du biome.
    /// </summary>
    public double Temperature { get; init; }

    /// <summary>
    /// Humidité absolue actuelle du biome.
    /// </summary>
    public double Humidity { get; init; }

    /// <summary>
    /// Nombre total d'espèces distinctes présentes dans le biome.
    /// </summary>
    public int TotalDistinctSpecies { get; init; }

    /// <summary>
    /// Données sérialisées en JSON utilisées par le JavaScript pour générer les graphiques.
    /// </summary>
    public string StatsJson { get; init; } = "{}";

    /// <summary>
    /// Liste des cartes KPI affichées en haut du tableau de bord.
    /// </summary>
    public IReadOnlyList<KpiCardVm> Kpis { get; init; } = Array.Empty<KpiCardVm>();

    /// <summary>
    /// Liste des métriques synthétiques du réseau trophique.
    /// </summary>
    public IReadOnlyList<NetworkMetricVm> NetworkMetrics { get; init; } = Array.Empty<NetworkMetricVm>();

    /// <summary>
    /// Liste des espèces affichées dans le tableau des variations critiques.
    /// </summary>
    public IReadOnlyList<CriticalSpeciesVm> CriticalSpecies { get; init; } = Array.Empty<CriticalSpeciesVm>();

    /// <summary>
    /// Crée un tableau de bord vide lorsqu'un biome ne possède pas encore de données exploitables.
    /// </summary>
    /// <param name="period">Période sélectionnée.</param>
    /// <param name="biomeName">Nom du biome.</param>
    /// <param name="biomeSlug">Slug du biome.</param>
    /// <returns>
    /// Un ViewModel vide mais correctement initialisé pour l'affichage.
    /// </returns>
    public static StatsDashboardVm Empty(string period, string biomeName, string biomeSlug)
    {
        return new StatsDashboardVm
        {
            Period = period,
            BiomeName = biomeName,
            BiomeSlug = biomeSlug,
            HasData = false,
            StatsJson = "{}"
        };
    }
}

/// <summary>
/// ViewModel représentant une carte KPI du tableau de bord.
/// </summary>
/// <param name="Title">Titre de l'indicateur.</param>
/// <param name="Value">Valeur principale affichée.</param>
/// <param name="Subtitle">Texte explicatif de l'indicateur.</param>
/// <param name="Delta">Badge d'évolution ou d'état.</param>
/// <param name="DeltaTone">Classe CSS utilisée pour colorer le badge.</param>
/// <param name="IconPath">Chemin de l'image affichée dans la carte.</param>
/// <param name="IconTone">Classe CSS utilisée pour colorer le fond de l'icône.</param>
public sealed record KpiCardVm(
    string Title,
    string Value,
    string Subtitle,
    string Delta,
    string DeltaTone,
    string IconPath,
    string IconTone);

/// <summary>
/// ViewModel représentant une métrique du réseau trophique.
/// </summary>
/// <param name="Title">Nom de la métrique.</param>
/// <param name="Value">Valeur calculée de la métrique.</param>
/// <param name="Status">Statut lisible associé à la valeur.</param>
/// <param name="StatusTone">Classe CSS utilisée pour colorer le statut.</param>
/// <param name="Description">Description courte de la métrique.</param>
/// <param name="CardTone">Classe CSS utilisée pour styliser la carte.</param>
public sealed record NetworkMetricVm(
    string Title,
    string Value,
    string Status,
    string StatusTone,
    string Description,
    string CardTone);

/// <summary>
/// ViewModel représentant une espèce affichée dans le tableau des espèces critiques.
/// </summary>
/// <param name="Name">Nom commun de l'espèce.</param>
/// <param name="ShortName">Initiale ou abréviation utilisée en fallback visuel.</param>
/// <param name="SubLabel">Sous-libellé affiché sous le nom, généralement le régime alimentaire.</param>
/// <param name="Classification">Classification de l'espèce.</param>
/// <param name="ClassificationTone">Classe CSS utilisée pour colorer le badge de classification.</param>
/// <param name="Population">Population formatée pour l'affichage.</param>
/// <param name="PopulationRaw">Population numérique brute.</param>
/// <param name="TrendLabel">Libellé de tendance affiché dans le tableau.</param>
/// <param name="TrendTone">Classe CSS utilisée pour colorer la tendance.</param>
/// <param name="TrendIcon">Nom de l'icône de tendance.</param>
/// <param name="TrendNumeric">Valeur numérique de la tendance.</param>
/// <param name="ImagePath">Chemin de l'image de l'espèce.</param>
/// <param name="Slug">Slug utilisé pour accéder à la fiche détaillée de l'espèce.</param>
public sealed record CriticalSpeciesVm(
    string Name,
    string ShortName,
    string SubLabel,
    string Classification,
    string ClassificationTone,
    string Population,
    int PopulationRaw,
    string TrendLabel,
    string TrendTone,
    string TrendIcon,
    double TrendNumeric,
    string ImagePath,
    string Slug);

/// <summary>
/// ViewModel utilisé par la partial d'affichage d'un graphique.
/// </summary>
/// <param name="Title">Titre du panneau.</param>
/// <param name="Description">Description affichée sous le titre.</param>
/// <param name="ChartId">Identifiant HTML du conteneur du graphique.</param>
/// <param name="ChartClass">Classe CSS appliquée au conteneur du graphique.</param>
/// <param name="CenterValue">Valeur affichée au centre d'un graphique donut.</param>
/// <param name="CenterLabel">Libellé affiché sous la valeur centrale.</param>
public sealed record ChartPanelVm(
    string Title,
    string Description,
    string ChartId,
    string ChartClass,
    string? CenterValue = null,
    string? CenterLabel = null);