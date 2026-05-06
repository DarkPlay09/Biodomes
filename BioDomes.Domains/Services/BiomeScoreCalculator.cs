namespace BioDomes.Domains.Services;

public static class BiomeScoreCalculator
{
    public static double Calculate(
        string? state,
        IEnumerable<string?> diets,
        int speciesCount,
        int equipmentCount)
    {
        var environmentScore = GetStateScore(state);
        var trophicScore = CalculateTrophicStability(diets);
        var speciesBonus = Math.Min(speciesCount * 4, 15);
        var equipmentBonus = Math.Min(equipmentCount * 3, 10);

        var score = environmentScore * 0.55
                    + trophicScore * 0.35
                    + speciesBonus
                    + equipmentBonus;

        return Math.Round(Math.Min(score, 100), 1);
    }

    public static int GetStateScore(string? state)
    {
        if (string.Equals(state, "Optimal", StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (string.Equals(state, "Instable", StringComparison.OrdinalIgnoreCase))
        {
            return 60;
        }

        if (string.Equals(state, "Critique", StringComparison.OrdinalIgnoreCase))
        {
            return 25;
        }

        return 50;
    }

    public static double CalculateTrophicStability(IEnumerable<string?> diets)
    {
        var dietList = diets
            .Select(diet => diet ?? string.Empty)
            .ToList();

        var hasProducer = dietList.Any(IsProducer);
        var hasHerbivore = dietList.Any(IsHerbivore) || dietList.Any(IsOmnivore);
        var hasCarnivore = dietList.Any(IsCarnivore) || dietList.Any(IsOmnivore);

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

    private static bool IsProducer(string diet)
    {
        return diet.Contains("photo", StringComparison.OrdinalIgnoreCase)
               || diet.Contains("autotroph", StringComparison.OrdinalIgnoreCase)
               || diet.Contains("producteur", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHerbivore(string diet)
    {
        return diet.Contains("herbivore", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCarnivore(string diet)
    {
        return diet.Contains("carnivore", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOmnivore(string diet)
    {
        return diet.Contains("omnivore", StringComparison.OrdinalIgnoreCase);
    }
}