using BioDomes.Domains.Enums;
using System.Globalization;

namespace BioDomes.Domains.Entities;

public class Biome
{
    public int Id { get; set; } // Primary key
    public string Name { get; private set; }
    public double Temperature { get; private set; }
    public double AbsoluteHumidity { get; private set; }
    public BiomeState State { get; private set; }
    public UserAccount Creator { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Biome(
        string name,
        double temperature,
        double absoluteHumidity,
        UserAccount creator,
        DateTime? updatedAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du biome est requis.", nameof(name));

        Name = name;
        Creator = creator ?? throw new ArgumentException("Le créateur du biome est requis.", nameof(creator));
        UpdatedAt = updatedAt ?? DateTime.UtcNow;

        UpdateConditions(temperature, absoluteHumidity);
    }

    public void UpdateConditions(double temperature, double absoluteHumidity)
    {
        EnsureDataIsValid(temperature, absoluteHumidity);

        Temperature = temperature;
        AbsoluteHumidity = absoluteHumidity;
        State = ComputeState(temperature);
    }

    public static double ComputeMaxAbsoluteHumidity(double temperature)
    {
        var exponent = (17.67 * temperature) / (temperature + 243.5);
        var numerator = 216.7 * 6.112 * Math.Exp(exponent);
        var denominator = temperature + 273.15;

        return numerator / denominator;
    }

    private static BiomeState ComputeState(double temperature)
    {
        return temperature switch
        {
            < -20 or > 40 => BiomeState.Critique,
            < 0 or > 30 => BiomeState.Instable,
            _ => BiomeState.Optimal
        };
    }

    private static void EnsureDataIsValid(double temperature, double absoluteHumidity)
    {
        if (absoluteHumidity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(absoluteHumidity), "L'humidité absolue ne peut pas être négative.");
        }

        if (temperature <= -100)
        {
            throw new ArgumentOutOfRangeException(nameof(temperature), "La température ne peut pas être inférieure à -100°C.");
        }

        var maxHumidity = ComputeMaxAbsoluteHumidity(temperature);
        if (absoluteHumidity <= maxHumidity)
            return;

        var fr = new CultureInfo("fr-BE");
        var formattedTemperature = temperature.ToString("0.##", fr);
        var formattedMaxHumidity = maxHumidity.ToString("0.##", fr);
        throw new InvalidOperationException(
            $"Un biome avec une température de {formattedTemperature}° a une humidité max de {formattedMaxHumidity} g/m³.");
    }
}
