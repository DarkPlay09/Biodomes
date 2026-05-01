using System.Globalization;
using System.Text.Json;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Services.Slug;
using BioDomes.Web.ViewModels.Stats;
using Microsoft.EntityFrameworkCore;

namespace BioDomes.Web.Services.Stats;

/// <summary>
/// Service responsable de la construction complète du tableau de bord statistique d'un biome.
/// Il récupère les données du biome, calcule les indicateurs écologiques,
/// prépare les séries utilisées par les graphiques et sérialise les données pour ECharts.
/// </summary>
public sealed class StatsDashboardService : IStatsDashboardService
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-BE");

    private readonly BioDomesDbContext _context;
    private readonly ISlugService _slugService;

    /// <summary>
    /// Initialise une nouvelle instance du service de statistiques.
    /// </summary>
    /// <param name="context">
    /// Contexte Entity Framework utilisé pour accéder aux biomes et à leurs espèces.
    /// </param>
    /// <param name="slugService">
    /// Service utilisé pour normaliser les slugs des biomes et des espèces.
    /// </param>
    public StatsDashboardService(BioDomesDbContext context, ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    /// <summary>
    /// Construit toutes les données nécessaires à l'affichage des statistiques d'un biome.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur connecté.</param>
    /// <param name="biomeSlug">Slug du biome à analyser.</param>
    /// <param name="period">Période sélectionnée dans l'interface.</param>
    /// <returns>
    /// Un ViewModel complet si le biome existe et appartient à l'utilisateur,
    /// sinon null.
    /// </returns>
    public async Task<StatsDashboardVm?> BuildForBiomeAsync(int userId, string biomeSlug, string period)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var normalizedSlug = _slugService.ToSlug(biomeSlug);

        var biomes = await _context.Biomes
            .AsNoTracking()
            .Where(b => b.CreatorId == userId)
            .Include(b => b.BiomeSpeciesLinks)
                .ThenInclude(l => l.Species)
            .OrderByDescending(b => b.UpdatedAt)
            .ToListAsync();

        var biome = biomes.FirstOrDefault(b => _slugService.ToSlug(b.Name) == normalizedSlug);

        if (biome is null)
        {
            return null;
        }

        var snapshot = BuildBiomeSnapshot(biome);
        var biomeSlugResult = _slugService.ToSlug(biome.Name);

        if (snapshot.Sources.Count == 0)
        {
            return new StatsDashboardVm
            {
                Period = normalizedPeriod,
                HasData = false,
                BiomeName = biome.Name,
                BiomeSlug = biomeSlugResult,
                BiomeState = biome.State,
                Temperature = biome.Temperature,
                Humidity = biome.AbsoluteHumidity,
                StatsJson = "{}"
            };
        }

        var criticalSpecies = BuildCriticalSpecies(snapshot.Sources);

        var payload = new ChartsPayload(
            CurrentPeriodLabel: GetCurrentPeriodLabel(normalizedPeriod),
            GeneratedAt: DateTime.Now.ToString("dd/MM/yyyy HH:mm", Fr),
            BiodiversityTimeline: BuildBiomeTimeline(biome),
            SpeciesSeries: BuildSpeciesSeries(snapshot.Sources),
            ClassificationSeries: BuildGroupedSeries(snapshot.Sources, source => source.Classification),
            DietSeries: BuildGroupedSeries(snapshot.Sources, source => source.Diet),
            TrophicGraph: BuildTrophicGraph(snapshot.Sources),
            CriticalSpecies: criticalSpecies
                .Select(species => new CriticalSpeciesExport(
                    species.Name,
                    species.Classification,
                    species.PopulationRaw,
                    species.TrendNumeric))
                .ToList());

        return new StatsDashboardVm
        {
            Period = normalizedPeriod,
            HasData = true,
            BiomeName = biome.Name,
            BiomeSlug = biomeSlugResult,
            BiomeState = biome.State,
            Temperature = biome.Temperature,
            Humidity = biome.AbsoluteHumidity,
            TotalDistinctSpecies = snapshot.DistinctSpecies,
            Kpis = BuildKpis(snapshot),
            NetworkMetrics = BuildNetworkMetrics(snapshot),
            CriticalSpecies = criticalSpecies,
            StatsJson = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
        };
    }

    /// <summary>
    /// Transforme un biome en snapshot statistique exploitable par les KPI et les graphiques.
    /// </summary>
    /// <param name="biome">Biome à analyser.</param>
    /// <returns>
    /// Snapshot contenant les espèces, populations et indicateurs calculés.
    /// </returns>
    private Snapshot BuildBiomeSnapshot(BiomeEntity biome)
    {
        var sources = biome.BiomeSpeciesLinks
            .Select(link => new SpeciesSource(
                BiomeId: biome.Id,
                BiomeName: biome.Name,
                UpdatedAt: biome.UpdatedAt,
                State: biome.State,
                SpeciesId: link.SpeciesId,
                SpeciesName: link.Species.Name,
                SpeciesSlug: _slugService.ToSlug(link.Species.Name),
                Classification: string.IsNullOrWhiteSpace(link.Species.Classification)
                    ? "Non renseigné"
                    : link.Species.Classification,
                Diet: string.IsNullOrWhiteSpace(link.Species.Diet)
                    ? "Non renseigné"
                    : link.Species.Diet,
                Weight: link.Species.Weight,
                IndividualCount: link.IndividualCount,
                ImagePath: string.IsNullOrWhiteSpace(link.Species.ImagePath)
                    ? "/uploads/species/noImageSpecie.png"
                    : link.Species.ImagePath))
            .ToList();

        var distinctSpecies = sources
            .Select(source => source.SpeciesId)
            .Distinct()
            .Count();

        var totalIndividuals = sources.Sum(source => source.IndividualCount);

        var biodiversityIndex = Math.Round(Math.Min(100.0, distinctSpecies * 10.0), 1);
        var trophicStability = Math.Round(CalculateTrophicStability(biome), 1);
        var environmentScore = GetStateScore(biome.State);
        var connectivity = Math.Round(CalculateConnectivity(sources), 2);
        var chainLength = Math.Round(CalculateChainLength(biome), 1);
        var predatorPreyRatio = Math.Round(CalculatePredatorPreyRatio(sources), 2);

        return new Snapshot(
            TotalBiomes: 1,
            DistinctSpecies: distinctSpecies,
            TotalIndividuals: totalIndividuals,
            AverageSpeciesPerBiome: distinctSpecies,
            BiodiversityIndex: biodiversityIndex,
            TrophicStability: trophicStability,
            EnvironmentScore: environmentScore,
            EnvironmentGrade: GetEnvironmentGrade(environmentScore),
            Connectivity: connectivity,
            ChainLength: chainLength,
            PredatorPreyRatio: predatorPreyRatio,
            Sources: sources);
    }

    /// <summary>
    /// Construit les cartes KPI principales affichées en haut de la page.
    /// </summary>
    /// <param name="snapshot">Snapshot statistique du biome.</param>
    /// <returns>Liste des KPI prêts à afficher.</returns>
    private static IReadOnlyList<KpiCardVm> BuildKpis(Snapshot snapshot)
    {
        return new List<KpiCardVm>
        {
            new(
                "Indice de Biodiversité",
                snapshot.BiodiversityIndex.ToString("0.0", Fr),
                $"{snapshot.DistinctSpecies} espèce(s) distincte(s) dans ce biome",
                "Actuel",
                "analytics-delta--neutral",
                "~/images/biomes/icons/loop.png",
                "analytics-tone--green"),

            new(
                "Population totale",
                snapshot.TotalIndividuals.ToString("N0", Fr),
                "Nombre total d’individus introduits",
                "Actuel",
                "analytics-delta--neutral",
                "~/images/dashboard/icons/paws.png",
                "analytics-tone--orange"),

            new(
                "Stabilité Trophique",
                $"{snapshot.TrophicStability.ToString("0.0", Fr)}%",
                "Présence estimée de producteurs, proies et prédateurs",
                GetHealthBadge(snapshot.TrophicStability),
                snapshot.TrophicStability >= 70
                    ? "analytics-delta--positive"
                    : "analytics-delta--warning",
                "~/images/biomes/icons/science.png",
                "analytics-tone--blue"),

            new(
                "Santé Environnementale",
                snapshot.EnvironmentGrade,
                $"Score du biome : {snapshot.EnvironmentScore.ToString("0", Fr)}/100",
                GetHealthBadge(snapshot.EnvironmentScore),
                "analytics-delta--neutral",
                "~/images/biomes/icons/temperature-stable.png",
                "analytics-tone--teal")
        };
    }

    /// <summary>
    /// Construit les indicateurs synthétiques du réseau trophique.
    /// </summary>
    /// <param name="snapshot">Snapshot statistique du biome.</param>
    /// <returns>Liste des métriques du réseau trophique.</returns>
    private static IReadOnlyList<NetworkMetricVm> BuildNetworkMetrics(Snapshot snapshot)
    {
        return new List<NetworkMetricVm>
        {
            new(
                "Connectivité",
                snapshot.Connectivity.ToString("0.00", Fr),
                GetConnectivityStatus(snapshot.Connectivity),
                GetConnectivityTone(snapshot.Connectivity),
                "Capacité estimée de circulation des relations trophiques.",
                "analytics-metric-card--primary"),

            new(
                "Long. chaîne",
                snapshot.ChainLength.ToString("0.0", Fr),
                snapshot.ChainLength >= 3 ? "Stable" : "Courte",
                snapshot.ChainLength >= 3
                    ? "analytics-delta--positive"
                    : "analytics-delta--warning",
                "Nombre moyen de niveaux trophiques détectés.",
                "analytics-metric-card--moss"),

            new(
                "Ratio P/P",
                snapshot.PredatorPreyRatio.ToString("0.00", Fr),
                snapshot.PredatorPreyRatio is >= 0.08 and <= 0.80 ? "Normal" : "Alerte",
                snapshot.PredatorPreyRatio is >= 0.08 and <= 0.80
                    ? "analytics-delta--positive"
                    : "analytics-delta--warning",
                "Rapport estimé entre biomasse prédatrice et biomasse de proies.",
                "analytics-metric-card--neutral")
        };
    }

    /// <summary>
    /// Construit la série temporelle utilisée par le graphique de biodiversité.
    /// </summary>
    /// <param name="biome">Biome analysé.</param>
    /// <returns>Liste de points temporels pour le graphique.</returns>
    private static IReadOnlyList<TimelinePoint> BuildBiomeTimeline(BiomeEntity biome)
    {
        var distinctSpecies = biome.BiomeSpeciesLinks
            .Select(link => link.SpeciesId)
            .Distinct()
            .Count();

        var totalIndividuals = biome.BiomeSpeciesLinks
            .Sum(link => link.IndividualCount);

        var biodiversityIndex = Math.Round(Math.Min(100.0, distinctSpecies * 10.0), 1);

        if (biome.CreatedAt.Date == biome.UpdatedAt.Date)
        {
            return new List<TimelinePoint>
            {
                new(
                    biome.UpdatedAt.ToString("dd/MM", Fr),
                    biodiversityIndex,
                    totalIndividuals)
            };
        }

        return new List<TimelinePoint>
        {
            new(
                biome.CreatedAt.ToString("dd/MM", Fr),
                biodiversityIndex,
                totalIndividuals),

            new(
                biome.UpdatedAt.ToString("dd/MM", Fr),
                biodiversityIndex,
                totalIndividuals)
        };
    }

    /// <summary>
    /// Construit la liste des espèces affichées dans le tableau des variations critiques.
    /// </summary>
    /// <param name="sources">Sources statistiques issues des espèces du biome.</param>
    /// <returns>Liste des espèces triées par population croissante.</returns>
    private static IReadOnlyList<CriticalSpeciesVm> BuildCriticalSpecies(IReadOnlyList<SpeciesSource> sources)
    {
        return sources
            .GroupBy(source => source.SpeciesId)
            .Select(group =>
            {
                var first = group.First();
                var population = group.Sum(source => source.IndividualCount);

                return new CriticalSpeciesVm(
                    Name: first.SpeciesName,
                    ShortName: GetShortName(first.SpeciesName),
                    SubLabel: first.Diet,
                    Classification: first.Classification,
                    ClassificationTone: GetClassificationTone(first.Classification),
                    Population: $"{population:N0} individus",
                    PopulationRaw: population,
                    TrendLabel: "Actuel",
                    TrendTone: "analytics-trend--neutral",
                    TrendIcon: "remove",
                    TrendNumeric: 0,
                    ImagePath: first.ImagePath,
                    Slug: first.SpeciesSlug);
            })
            .OrderBy(species => species.PopulationRaw)
            .ThenBy(species => species.Name)
            .Take(8)
            .ToList();
    }

    /// <summary>
    /// Regroupe les populations par espèce pour alimenter le graphique en barres.
    /// </summary>
    /// <param name="sources">Sources statistiques du biome.</param>
    /// <returns>Série de données par espèce.</returns>
    private static IReadOnlyList<ChartSlice> BuildSpeciesSeries(
        IReadOnlyList<SpeciesSource> sources)
    {
        var groups = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group => new
            {
                Label = group.First().SpeciesName,
                Value = group.Sum(source => source.IndividualCount),
                SpeciesCount = 1,
                BiomeCount = 1
            })
            .OrderByDescending(group => group.Value)
            .ThenBy(group => group.Label)
            .ToList();

        var total = groups.Sum(group => group.Value);

        return groups
            .Select(group => new ChartSlice(
                group.Label,
                group.Value,
                total == 0 ? 0 : Math.Round(group.Value * 100.0 / total, 1),
                group.SpeciesCount,
                group.BiomeCount))
            .ToList();
    }

    /// <summary>
    /// Regroupe les populations selon un critère donné, comme la classification ou le régime alimentaire.
    /// </summary>
    /// <param name="sources">Sources statistiques du biome.</param>
    /// <param name="groupSelector">Fonction indiquant le champ de regroupement.</param>
    /// <returns>Série de données regroupées.</returns>
    private static IReadOnlyList<ChartSlice> BuildGroupedSeries(
        IReadOnlyList<SpeciesSource> sources,
        Func<SpeciesSource, string> groupSelector)
    {
        var groups = sources
            .GroupBy(groupSelector)
            .Select(group => new
            {
                Label = string.IsNullOrWhiteSpace(group.Key)
                    ? "Non renseigné"
                    : group.Key,
                Value = group.Sum(source => source.IndividualCount),
                SpeciesCount = group.Select(source => source.SpeciesId).Distinct().Count(),
                BiomeCount = 1
            })
            .OrderByDescending(group => group.Value)
            .ThenBy(group => group.Label)
            .ToList();

        var total = groups.Sum(group => group.Value);

        return groups
            .Select(group => new ChartSlice(
                group.Label,
                group.Value,
                total == 0 ? 0 : Math.Round(group.Value * 100.0 / total, 1),
                group.SpeciesCount,
                group.BiomeCount))
            .ToList();
    }

    /// <summary>
    /// Construit un graphe trophique simplifié à partir des espèces présentes dans le biome.
    /// </summary>
    /// <param name="sources">Sources statistiques du biome.</param>
    /// <returns>Graphe contenant les nœuds des espèces et leurs relations trophiques.</returns>
    private static TrophicGraph BuildTrophicGraph(IReadOnlyList<SpeciesSource> sources)
    {
        var nodes = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group =>
            {
                var first = group.First();
                var individualCount = group.Sum(source => source.IndividualCount);

                return new TrophicNode(
                    Id: $"species-{first.SpeciesId}",
                    Name: first.SpeciesName,
                    Category: GetDietCategory(first.Diet),
                    Value: individualCount,
                    Diet: first.Diet,
                    SymbolSize: Math.Clamp(
                        28 + Math.Sqrt(Math.Max(individualCount, 1)) * 3,
                        32,
                        74));
            })
            .OrderBy(node => node.Name)
            .ToList();

        var links = new List<TrophicLink>();
        var linkKeys = new HashSet<string>();

        var speciesInBiome = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group => group.First())
            .ToList();

        var producers = speciesInBiome
            .Where(source => IsProducer(source.Diet))
            .ToList();

        var herbivores = speciesInBiome
            .Where(source => IsHerbivore(source.Diet) || IsOmnivore(source.Diet))
            .ToList();

        var carnivores = speciesInBiome
            .Where(source => IsCarnivore(source.Diet) || IsOmnivore(source.Diet))
            .ToList();

        AddLinks(producers, herbivores, "nourrit");
        AddLinks(herbivores, carnivores, "proie de");

        return new TrophicGraph(nodes, links);

        void AddLinks(
            IReadOnlyList<SpeciesSource> sourceList,
            IReadOnlyList<SpeciesSource> targetList,
            string relation)
        {
            foreach (var source in sourceList)
            {
                foreach (var target in targetList)
                {
                    if (source.SpeciesId == target.SpeciesId)
                    {
                        continue;
                    }

                    var key = $"{source.SpeciesId}-{target.SpeciesId}-{relation}";

                    if (!linkKeys.Add(key))
                    {
                        continue;
                    }

                    links.Add(new TrophicLink(
                        Source: $"species-{source.SpeciesId}",
                        Target: $"species-{target.SpeciesId}",
                        Label: $"{source.SpeciesName} {relation} {target.SpeciesName}",
                        BiomeName: source.BiomeName));
                }
            }
        }
    }

    /// <summary>
    /// Calcule un score de stabilité trophique selon la présence de producteurs,
    /// d'herbivores et de carnivores dans le biome.
    /// </summary>
    /// <param name="biome">Biome analysé.</param>
    /// <returns>Score de stabilité compris entre 0 et 100.</returns>
    private static double CalculateTrophicStability(BiomeEntity biome)
    {
        var diets = biome.BiomeSpeciesLinks
            .Select(link => link.Species.Diet ?? string.Empty)
            .ToList();

        var hasProducer = diets.Any(IsProducer);
        var hasHerbivore = diets.Any(IsHerbivore) || diets.Any(IsOmnivore);
        var hasCarnivore = diets.Any(IsCarnivore) || diets.Any(IsOmnivore);

        double score = 20;

        if (hasProducer)
        {
            score += 30;
        }

        if (hasHerbivore)
        {
            score += 25;
        }

        if (hasCarnivore)
        {
            score += 25;
        }

        return Math.Min(score, 100);
    }

    /// <summary>
    /// Estime la longueur moyenne de la chaîne trophique du biome.
    /// </summary>
    /// <param name="biome">Biome analysé.</param>
    /// <returns>Longueur trophique estimée.</returns>
    private static double CalculateChainLength(BiomeEntity biome)
    {
        var diets = biome.BiomeSpeciesLinks
            .Select(link => link.Species.Diet ?? string.Empty)
            .ToList();

        var length = 1.0;

        if (diets.Any(IsProducer))
        {
            length += 1.0;
        }

        if (diets.Any(IsHerbivore) || diets.Any(IsOmnivore))
        {
            length += 1.0;
        }

        if (diets.Any(IsCarnivore))
        {
            length += 1.0;
        }

        return length;
    }

    /// <summary>
    /// Calcule la connectivité du réseau trophique à partir des relations possibles entre espèces.
    /// </summary>
    /// <param name="sources">Sources statistiques du biome.</param>
    /// <returns>Valeur de connectivité comprise entre 0 et 1.</returns>
    private static double CalculateConnectivity(IReadOnlyList<SpeciesSource> sources)
    {
        var speciesIds = sources
            .Select(source => source.SpeciesId)
            .Distinct()
            .ToList();

        if (speciesIds.Count <= 1)
        {
            return 0;
        }

        var links = new HashSet<string>();

        var producers = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group => group.First())
            .Where(source => IsProducer(source.Diet))
            .ToList();

        var herbivores = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group => group.First())
            .Where(source => IsHerbivore(source.Diet) || IsOmnivore(source.Diet))
            .ToList();

        var carnivores = sources
            .GroupBy(source => source.SpeciesId)
            .Select(group => group.First())
            .Where(source => IsCarnivore(source.Diet) || IsOmnivore(source.Diet))
            .ToList();

        AddConnectivityLinks(producers, herbivores);
        AddConnectivityLinks(herbivores, carnivores);

        var maxPossible = speciesIds.Count * (speciesIds.Count - 1);

        return maxPossible == 0
            ? 0
            : Math.Min(1, links.Count / (double)maxPossible);

        void AddConnectivityLinks(
            IReadOnlyList<SpeciesSource> sourceList,
            IReadOnlyList<SpeciesSource> targetList)
        {
            foreach (var source in sourceList)
            {
                foreach (var target in targetList)
                {
                    if (source.SpeciesId != target.SpeciesId)
                    {
                        links.Add($"{source.SpeciesId}-{target.SpeciesId}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calcule le rapport entre la biomasse des prédateurs et celle des proies.
    /// </summary>
    /// <param name="sources">Sources statistiques du biome.</param>
    /// <returns>Ratio prédateurs/proies.</returns>
    private static double CalculatePredatorPreyRatio(IReadOnlyList<SpeciesSource> sources)
    {
        var predatorBiomass = sources
            .Where(source => IsCarnivore(source.Diet))
            .Sum(source => source.Weight * source.IndividualCount);

        var preyBiomass = sources
            .Where(source => IsHerbivore(source.Diet) || IsOmnivore(source.Diet))
            .Sum(source => source.Weight * source.IndividualCount);

        if (preyBiomass <= 0)
        {
            return 0;
        }

        return predatorBiomass / preyBiomass;
    }

    /// <summary>
    /// Normalise la période reçue depuis l'URL afin d'éviter les valeurs invalides.
    /// </summary>
    /// <param name="period">Période reçue.</param>
    /// <returns>Période normalisée.</returns>
    private static string NormalizePeriod(string period)
    {
        return period switch
        {
            "1" => "1",
            "7" => "7",
            "30" => "30",
            "365" => "365",
            "all" => "all",
            _ => "30"
        };
    }

    /// <summary>
    /// Convertit une période technique en libellé lisible pour l'interface.
    /// </summary>
    /// <param name="period">Période normalisée.</param>
    /// <returns>Libellé français de la période.</returns>
    private static string GetCurrentPeriodLabel(string period)
    {
        return period switch
        {
            "1" => "Dernières 24h",
            "7" => "7 jours",
            "30" => "30 jours",
            "365" => "1 an",
            "all" => "Toutes les données",
            _ => "30 jours"
        };
    }

    /// <summary>
    /// Convertit l'état textuel d'un biome en score numérique environnemental.
    /// </summary>
    /// <param name="state">État du biome.</param>
    /// <returns>Score environnemental associé.</returns>
    private static int GetStateScore(string state)
    {
        if (state.Equals("Optimal", StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (state.Equals("Instable", StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        if (state.Equals("Critique", StringComparison.OrdinalIgnoreCase))
        {
            return 25;
        }

        return 50;
    }

    /// <summary>
    /// Convertit un score environnemental en note lisible.
    /// </summary>
    /// <param name="score">Score environnemental.</param>
    /// <returns>Note environnementale.</returns>
    private static string GetEnvironmentGrade(double score)
    {
        if (score >= 90)
        {
            return "A+";
        }

        if (score >= 80)
        {
            return "A";
        }

        if (score >= 70)
        {
            return "B";
        }

        if (score >= 60)
        {
            return "C";
        }

        return "D";
    }

    /// <summary>
    /// Détermine un badge d'état selon un score numérique.
    /// </summary>
    /// <param name="score">Score à évaluer.</param>
    /// <returns>Libellé du badge.</returns>
    private static string GetHealthBadge(double score)
    {
        return score >= 80
            ? "Stable"
            : score >= 60
                ? "Surveiller"
                : "Fragile";
    }

    /// <summary>
    /// Détermine le statut textuel de la connectivité trophique.
    /// </summary>
    /// <param name="connectivity">Valeur de connectivité.</param>
    /// <returns>Statut de connectivité.</returns>
    private static string GetConnectivityStatus(double connectivity)
    {
        return connectivity >= 0.15
            ? "Optimal"
            : connectivity >= 0.07
                ? "Moyen"
                : "Faible";
    }

    /// <summary>
    /// Détermine la classe CSS associée au niveau de connectivité.
    /// </summary>
    /// <param name="connectivity">Valeur de connectivité.</param>
    /// <returns>Classe CSS du badge.</returns>
    private static string GetConnectivityTone(double connectivity)
    {
        return connectivity >= 0.15
            ? "analytics-delta--positive"
            : connectivity >= 0.07
                ? "analytics-delta--warning"
                : "analytics-delta--negative";
    }

    /// <summary>
    /// Détermine la classe CSS associée à une classification d'espèce.
    /// </summary>
    /// <param name="classification">Classification de l'espèce.</param>
    /// <returns>Classe CSS du badge de classification.</returns>
    private static string GetClassificationTone(string classification)
    {
        if (classification.Contains("mamm", StringComparison.OrdinalIgnoreCase))
        {
            return "analytics-tag--blue";
        }

        if (classification.Contains("oise", StringComparison.OrdinalIgnoreCase))
        {
            return "analytics-tag--orange";
        }

        if (classification.Contains("rept", StringComparison.OrdinalIgnoreCase))
        {
            return "analytics-tag--green";
        }

        if (classification.Contains("amphi", StringComparison.OrdinalIgnoreCase))
        {
            return "analytics-tag--mint";
        }

        if (classification.Contains("plante", StringComparison.OrdinalIgnoreCase))
        {
            return "analytics-tag--green";
        }

        return "analytics-tag--neutral";
    }

    /// <summary>
    /// Récupère l'initiale d'un nom pour l'affichage de secours.
    /// </summary>
    /// <param name="name">Nom de l'espèce.</param>
    /// <returns>Initiale en majuscule ou ? si le nom est vide.</returns>
    private static string GetShortName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        return name.Trim()[0].ToString().ToUpperInvariant();
    }

    /// <summary>
    /// Convertit un régime alimentaire en catégorie numérique utilisée par le graphe ECharts.
    /// </summary>
    /// <param name="diet">Régime alimentaire.</param>
    /// <returns>Identifiant de catégorie trophique.</returns>
    private static int GetDietCategory(string diet)
    {
        if (IsProducer(diet))
        {
            return 0;
        }

        if (IsHerbivore(diet))
        {
            return 1;
        }

        if (IsCarnivore(diet))
        {
            return 2;
        }

        if (IsOmnivore(diet))
        {
            return 3;
        }

        return 4;
    }

    /// <summary>
    /// Indique si un régime correspond à un producteur primaire.
    /// </summary>
    /// <param name="diet">Régime alimentaire.</param>
    /// <returns>True si le régime correspond à un producteur.</returns>
    private static bool IsProducer(string diet)
    {
        return diet.Contains("photo", StringComparison.OrdinalIgnoreCase)
            || diet.Contains("autotroph", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indique si un régime correspond à un herbivore.
    /// </summary>
    /// <param name="diet">Régime alimentaire.</param>
    /// <returns>True si le régime correspond à un herbivore.</returns>
    private static bool IsHerbivore(string diet)
    {
        return diet.Contains("herbivore", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indique si un régime correspond à un carnivore.
    /// </summary>
    /// <param name="diet">Régime alimentaire.</param>
    /// <returns>True si le régime correspond à un carnivore.</returns>
    private static bool IsCarnivore(string diet)
    {
        return diet.Contains("carnivore", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indique si un régime correspond à un omnivore.
    /// </summary>
    /// <param name="diet">Régime alimentaire.</param>
    /// <returns>True si le régime correspond à un omnivore.</returns>
    private static bool IsOmnivore(string diet)
    {
        return diet.Contains("omnivore", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Représente une espèce du biome sous une forme exploitable pour les calculs statistiques.
    /// </summary>
    private sealed record SpeciesSource(
        int BiomeId,
        string BiomeName,
        DateTime UpdatedAt,
        string State,
        int SpeciesId,
        string SpeciesName,
        string SpeciesSlug,
        string Classification,
        string Diet,
        double Weight,
        int IndividualCount,
        string ImagePath);

    /// <summary>
    /// Représente l'ensemble des indicateurs calculés pour un biome à un instant donné.
    /// </summary>
    private sealed record Snapshot(
        int TotalBiomes,
        int DistinctSpecies,
        int TotalIndividuals,
        double AverageSpeciesPerBiome,
        double BiodiversityIndex,
        double TrophicStability,
        double EnvironmentScore,
        string EnvironmentGrade,
        double Connectivity,
        double ChainLength,
        double PredatorPreyRatio,
        IReadOnlyList<SpeciesSource> Sources);

    /// <summary>
    /// Représente le payload JSON transmis au JavaScript pour générer les graphiques ECharts.
    /// </summary>
    private sealed record ChartsPayload(
        string CurrentPeriodLabel,
        string GeneratedAt,
        IReadOnlyList<TimelinePoint> BiodiversityTimeline,
        IReadOnlyList<ChartSlice> SpeciesSeries,
        IReadOnlyList<ChartSlice> ClassificationSeries,
        IReadOnlyList<ChartSlice> DietSeries,
        TrophicGraph TrophicGraph,
        IReadOnlyList<CriticalSpeciesExport> CriticalSpecies);

    /// <summary>
    /// Représente un point de la courbe de biodiversité dans le temps.
    /// </summary>
    private sealed record TimelinePoint(
        string Date,
        double BiodiversityIndex,
        int IndividualCount);

    /// <summary>
    /// Représente une tranche ou une barre de graphique.
    /// </summary>
    private sealed record ChartSlice(
        string Label,
        int Value,
        double Percentage,
        int SpeciesCount,
        int BiomeCount);

    /// <summary>
    /// Représente le graphe trophique complet utilisé par ECharts.
    /// </summary>
    private sealed record TrophicGraph(
        IReadOnlyList<TrophicNode> Nodes,
        IReadOnlyList<TrophicLink> Links);

    /// <summary>
    /// Représente un nœud du graphe trophique, généralement une espèce.
    /// </summary>
    private sealed record TrophicNode(
        string Id,
        string Name,
        int Category,
        int Value,
        string Diet,
        double SymbolSize);

    /// <summary>
    /// Représente une relation entre deux espèces dans le graphe trophique.
    /// </summary>
    private sealed record TrophicLink(
        string Source,
        string Target,
        string Label,
        string BiomeName);

    /// <summary>
    /// Représente une espèce exportée dans les fichiers CSV ou JSON.
    /// </summary>
    private sealed record CriticalSpeciesExport(
        string Name,
        string Classification,
        int Population,
        double Trend);
}