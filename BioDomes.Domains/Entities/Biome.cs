using BioDomes.Domains.Enums;
using System.Globalization;

namespace BioDomes.Domains.Entities;

public class Biome
{
    public string Name { get; private set; }
    public double Temperature { get; private set; }
    public double AbsoluteHumidity { get; private set; }
    public BiomeState State { get; private set; }
    public UserAccount Creator { get; private set; }

    public Biome(
        string name,
        double temperature,
        double absoluteHumidity,
        UserAccount creator) 
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du biome est requis.", nameof(name));

        Name = name;
        Creator = creator;

        UpdateConditions(temperature, absoluteHumidity);
    }

    public void UpdateConditions(double temperature, double absoluteHumidity)
    {
        EnsureHumidityIsValid(temperature, absoluteHumidity);

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

    private static void EnsureHumidityIsValid(double temperature, double absoluteHumidity)
    {
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
